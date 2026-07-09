
import subprocess
import webbrowser
import time
import os
import sys
import threading
import logging
import shutil
import re
from pathlib import Path
from typing import Optional, Tuple
import platform

FRONTEND_ACTUAL_URL: Optional[str] = None
frontend_url_lock = threading.Lock()

logger = logging.getLogger(__name__)

# Configuration
API_HOST = os.getenv("SMAS_API_HOST", "localhost")
API_PORT = os.getenv("SMAS_API_PORT", "5000")
FRONTEND_HOST = os.getenv("SMAS_FRONTEND_HOST", "localhost")
FRONTEND_PORT = os.getenv("SMAS_FRONTEND_PORT", "3000")

API_URL = f"http://{API_HOST}:{API_PORT}"
FRONTEND_URL = f"http://{FRONTEND_HOST}:{FRONTEND_PORT}"
API_HEALTH_URL = f"{API_URL}/health"


def load_env(root_dir: Path) -> None:
    """Load variables from .env file into os.environ and map to .NET."""
    env_file = root_dir / ".env"
    if env_file.exists():
# Password is supplied via DB_CONNECTION_STRING; skipping interactive prompt

        logger.info("Loading environment variables from .env...")
        loaded_vars = []
        with open(env_file) as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    k, v = line.split('=', 1)
                    os.environ[k] = v
                    # Log the key (but not sensitive values)
                    if k in ['DB_HOST', 'DB_PORT', 'DB_USER', 'TARGET_DB', 'JWT_ISSUER', 'JWT_AUDIENCE']:
                        logger.info(f"  {k}={v}")
                    else:
                        logger.info(f"  {k}=***")
                    loaded_vars.append(k)
                    
        logger.info(f"[OK] Loaded {len(loaded_vars)} environment variables")
        
        # Bridge .env to .NET standard environment variables
        if "DB_CONNECTION_STRING" in os.environ:
            os.environ["ConnectionStrings__DefaultConnection"] = os.environ["DB_CONNECTION_STRING"]
        if "JWT_KEY" in os.environ:
            os.environ["Jwt__Key"] = os.environ["JWT_KEY"]
        if "JWT_ISSUER" in os.environ:
            os.environ["Jwt__Issuer"] = os.environ["JWT_ISSUER"]
        if "JWT_AUDIENCE" in os.environ:
            os.environ["Jwt__Audience"] = os.environ["JWT_AUDIENCE"]
    else:
        logger.warning(".env file not found. Falling back to default appsettings.json.")


def configure_logging(log_file: Optional[str] = None) -> None:
    """Configure root logger with console and optional file handlers."""
    logger_root = logging.getLogger()
    logger_root.setLevel(logging.INFO)
    formatter = logging.Formatter(
        '[%(asctime)s] [%(levelname)s] %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )

    # Log the DB connection string being used (obscure password)
    logger.info(f"[INFO] DB connection string: {os.getenv('DB_CONNECTION_STRING', 'not set')}")

    if not logger_root.handlers:
        console_handler = logging.StreamHandler()
        console_handler.setFormatter(formatter)
        logger_root.addHandler(console_handler)

    if log_file:
        log_dir = Path(log_file).parent
        log_dir.mkdir(parents=True, exist_ok=True)
        file_handler = logging.FileHandler(log_file)
        file_handler.setFormatter(formatter)
        logger_root.addHandler(file_handler)


def check_command_exists(cmd: str) -> bool:
    """Check if a command exists in PATH."""
    return shutil.which(cmd) is not None


def _try_parse_frontend_url(line: str) -> None:
    global FRONTEND_ACTUAL_URL
    match = re.search(r'Local:\s*(http://\S+)', line)
    if match:
        url = match.group(1).rstrip('/')
        with frontend_url_lock:
            FRONTEND_ACTUAL_URL = url


def stream_process_output(process, prefix: str) -> None:
    """Stream process output to stdout with prefix."""
    try:
        for line in iter(process.stdout.readline, b''):
            try:
                decoded = line.decode('utf-8', errors='ignore').rstrip()
                if decoded:
                    print(f"[{prefix}] {decoded}")
                    sys.stdout.flush()
                    if prefix == "Frontend":
                        _try_parse_frontend_url(decoded)
            except Exception as e:
                logger.debug(f"Error decoding {prefix} output: {e}")
    except Exception as e:
        logger.debug(f"Stream output error for {prefix}: {e}")


def wait_for_url(url: str, timeout: int = 30, interval: float = 2.0) -> bool:
    """Wait for URL to become available. Returns True if ready, False if timeout."""
    import requests
    
    start = time.time()
    while time.time() - start < timeout:
        try:
            response = requests.get(url, timeout=5)
            if response.status_code == 200:
                logger.info(f"[OK] {url} is ready")
                return True
        except requests.RequestException:
            pass
        time.sleep(interval)
    
    logger.warning(f"[FAIL] {url} did not respond within {timeout}s")
    return False


def terminate_process(process, name: str) -> None:
    """Terminate a process gracefully, then forcefully if needed."""
    try:
        if sys.platform == 'win32':
            # Windows: use taskkill
            subprocess.run(
                ["taskkill", "/F", "/T", "/PID", str(process.pid)],
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                timeout=5
            )
        else:
            # Unix: terminate, then kill if needed
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()
        
        logger.info(f"[OK] {name} stopped")
    except Exception as e:
        logger.warning(f"Error stopping {name}: {e}")


def _find_chrome_executable() -> str | None:
    """Return path to Chrome executable if found, else None."""
    # Common Windows locations
    candidates = []
    if sys.platform == 'win32':
        candidates = [
            r"C:\Program Files\Google\Chrome\Application\chrome.exe",
            r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        ]
    else:
        # Unix-like
        candidates = [
            "/usr/bin/google-chrome",
            "/usr/bin/google-chrome-stable",
            "/usr/bin/chrome",
            "/snap/bin/chromium",
        ]

    # Check PATH
    which_name = shutil.which("chrome") or shutil.which("google-chrome") or shutil.which("chromium")
    if which_name:
        return which_name

    for path in candidates:
        if Path(path).exists():
            return path

    return None


def open_chrome(url: str) -> None:
    """Try to open the given URL in Chrome; fall back to the system default browser."""
    chrome = _find_chrome_executable()
    try:
        if chrome:
            # Use subprocess to open a new window/tab in Chrome
            subprocess.Popen([chrome, url], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            logger.info("Opened Chrome with URL")
            return
    except Exception as e:
        logger.debug(f"Failed to open Chrome directly: {e}")

    # Fallback to default browser
    webbrowser.open(url)


def main() -> int:
    """Main entry point for SMAS startup."""
    logger.info("=" * 60)
    logger.info("SMAS (Stock Monitoring and Analytic System)")
    logger.info("=" * 60)

    root_dir = Path(__file__).resolve().parent
    api_dir = root_dir / "SMAS.API"
    frontend_dir = root_dir / "frontend"

    # Environment Validation & Setup
    load_env(root_dir)

    # Validation
    logger.info("\n[1/5] Validating prerequisites...")
    
    if not check_command_exists("dotnet"):
        logger.error("dotnet CLI not found in PATH. Install .NET SDK 8.0+ and try again.")
        return 1
    
    if not check_command_exists("npm"):
        logger.error("npm not found in PATH. Install Node.js and try again.")
        return 1
    
    if not api_dir.is_dir():
        logger.error(f"API directory not found: {api_dir}")
        return 1
    
    if not frontend_dir.is_dir():
        logger.error(f"Frontend directory not found: {frontend_dir}")
        return 1

    logger.info("[OK] All prerequisites found")

    # Processes
    api_process: Optional[subprocess.Popen] = None
    frontend_process: Optional[subprocess.Popen] = None
    
    # Prepare environment for subprocesses
    env = os.environ.copy()

    try:
        # 2. Build Backend
        logger.info("\n[2/5] Building Backend API...")
        result = subprocess.run(
            ["dotnet", "build", "-v", "minimal"],
            cwd=api_dir,
            env=env,
            timeout=300
        )
        if result.returncode != 0:
            logger.error("Backend build failed")
            return 1
        logger.info("[OK] Backend built successfully")

        # 3. Start Backend API
        logger.info("\n[3/5] Starting Backend API...")
        api_process = subprocess.Popen(
            ["dotnet", "run"],
            cwd=api_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=False,
            env=env
        )
        api_thread = threading.Thread(
            target=stream_process_output,
            args=(api_process, "API"),
            daemon=True
        )
        api_thread.start()
        logger.info("Backend started (PID: {})".format(api_process.pid))

        # 4. Install and build frontend assets
        logger.info("\n[4/5] Installing and building Frontend...")
        npm_cmd = "npm.cmd" if sys.platform == "win32" else "npm"
        frontend_env = env.copy()
        frontend_env["BROWSER"] = "none"

        result = subprocess.run(
            [npm_cmd, "install"],
            cwd=frontend_dir,
            capture_output=True,
            timeout=300,
            env=frontend_env
        )
        if result.returncode != 0:
            logger.error("Frontend npm install failed")
            logger.error(result.stderr.decode('utf-8', errors='ignore'))
            return 1
        logger.info("[OK] Frontend dependencies installed")

        result = subprocess.run(
            [npm_cmd, "run", "build"],
            cwd=frontend_dir,
            capture_output=True,
            timeout=300,
            env=frontend_env
        )
        if result.returncode != 0:
            logger.error("Frontend build failed")
            logger.error(result.stderr.decode('utf-8', errors='ignore'))
            return 1
        logger.info("[OK] Frontend built successfully")

        # 5. Start frontend preview server
        logger.info("\n[5/5] Starting Frontend Preview Server...")
        frontend_process = subprocess.Popen(
            [npm_cmd, "run", "preview", "--", "--host", FRONTEND_HOST, "--port", FRONTEND_PORT],
            cwd=frontend_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=False,
            env=frontend_env
        )
        frontend_thread = threading.Thread(
            target=stream_process_output,
            args=(frontend_process, "Frontend"),
            daemon=True
        )
        frontend_thread.start()
        logger.info("Frontend preview started (PID: {})".format(frontend_process.pid))

        # 6. Health checks
        logger.info("\n[6/6] Waiting for services to be ready...")
        time.sleep(3)  # Initial wait for startup

        with frontend_url_lock:
            actual_frontend_url = FRONTEND_ACTUAL_URL or FRONTEND_URL

        api_ready = wait_for_url(API_HEALTH_URL, timeout=45)
        frontend_ready = wait_for_url(actual_frontend_url, timeout=30)

        if not (api_ready and frontend_ready):
            logger.warning("One or more services failed to start. Check logs above.")
        else:
            logger.info("[OK] All services are ready")

        # Open browser (prefer Chrome)
        logger.info(f"\n>> Opening SMAS in browser: {actual_frontend_url}")
        open_chrome(actual_frontend_url)

        # Print summary
        logger.info("\n" + "=" * 60)
        logger.info("[OK] SMAS is now running!")
        logger.info(f"  Frontend: {FRONTEND_URL}")
        logger.info(f"  API Docs: {API_HEALTH_URL}")
        logger.info("  Press Ctrl+C to stop the servers")
        logger.info("=" * 60 + "\n")

        # Keep running until interrupted
        while True:
            time.sleep(1)

    except KeyboardInterrupt:
        logger.info("\n>> Shutting down services...")

    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return 1

    finally:
        # Cleanup
        if api_process:
            terminate_process(api_process, "API")
        if frontend_process:
            terminate_process(frontend_process, "Frontend")
        logger.info(">> Goodbye!")

    return 0


if __name__ == "__main__":
    # Setup logging
    logs_dir = Path(__file__).parent / "logs"
    configure_logging(log_file=str(logs_dir / "run_smas.log"))
    
    exit_code = main()
    sys.exit(exit_code)
