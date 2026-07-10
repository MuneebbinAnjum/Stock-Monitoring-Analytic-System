#!/usr/bin/env python3
"""
setup_all.py

SMAS project setup automation - installs all project dependencies.

Responsibilities:
1. Creates Python virtual environment
2. Installs frontend (npm) dependencies
3. Installs backend (.NET) dependencies

Note: Database creation and migrations are handled automatically by EF Core on app startup.

Usage:
    python setup_all.py
"""

from __future__ import annotations

import os
import sys
import subprocess
import shutil
import logging
from pathlib import Path
from typing import List, Tuple
from datetime import datetime

try:
    import bcrypt
except ImportError:
    pass

# Configuration
logging.basicConfig(level=logging.INFO, format='[%(levelname)s] %(message)s')
logger = logging.getLogger(__name__)

ROOT = Path(__file__).resolve().parent

# Database configuration variables will be loaded from .env
TARGET_DB = os.getenv('TARGET_DB', 'smas_db')

def load_env() -> None:
    """Load variables from .env file into os.environ and map to .NET."""
    env_file = ROOT / ".env"
    if env_file.exists():
        # Password is now supplied via connection string; skipping interactive prompt
        # Ensure DB_CONNECTION_STRING includes the password
        pass

        with open(env_file) as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    k, v = line.split('=', 1)
                    os.environ[k] = v
                    
        # Bridge .env to .NET standard environment variables
        if "DB_CONNECTION_STRING" in os.environ:
            os.environ["ConnectionStrings__DefaultConnection"] = os.environ["DB_CONNECTION_STRING"]
        if "JWT_KEY" in os.environ:
            os.environ["Jwt__Key"] = os.environ["JWT_KEY"]
        if "JWT_ISSUER" in os.environ:
            os.environ["Jwt__Issuer"] = os.environ["JWT_ISSUER"]
        if "JWT_AUDIENCE" in os.environ:
            os.environ["Jwt__Audience"] = os.environ["JWT_AUDIENCE"]


def run_command(cmd: List[str], cwd: Path | None = None, env=None, check: bool = True, timeout: int = 600) -> int:
    """Execute a command and return exit code."""
    logger.info(f"Running: {' '.join(str(c) for c in cmd)}")
    try:
        result = subprocess.run(
            cmd,
            cwd=cwd or ROOT,
            env=env,
            check=check,
            capture_output=True,
            text=True,
            timeout=timeout
        )
        # Print output for visibility
        if result.stdout:
            print(result.stdout)
        if result.stderr and result.returncode != 0:
            print(result.stderr)
        return result.returncode
    except subprocess.TimeoutExpired:
        logger.error(f"Command timed out after {timeout} seconds: {' '.join(str(c) for c in cmd)}")
        if check:
            sys.exit(1)
        return 1
    except subprocess.CalledProcessError as e:
        logger.error(f"Command failed with exit code {e.returncode}")
        if e.stderr:
            logger.error(e.stderr)
        if check:
            sys.exit(1)
        return e.returncode


def install_python_requirements() -> None:
    """Install Python dependencies globally from requirements files."""
    req_files = list(ROOT.glob("requirements*.txt"))
    
    if not req_files:
        logger.info("No requirements*.txt files found; skipping Python package install")
        return

    pip_cmd = [sys.executable, "-m", "pip"]
    for req_file in req_files:
        logger.info(f"Installing dependencies from {req_file.name}")
        run_command(pip_cmd + ["install", "-r", str(req_file)])


def install_node_dependencies() -> None:
    """Install frontend (Node.js) dependencies with robust cleanup and retry."""
    frontend = ROOT / "frontend"
    pkg = frontend / "package.json"

    if not pkg.exists():
        logger.warning(f"package.json not found at {frontend}")
        return

    npm = shutil.which("npm")
    if not npm:
        logger.error("npm not found in PATH. Install Node.js and try again.")
        sys.exit(1)

    node_modules = frontend / "node_modules"
    # Ensure a clean state: delete existing node_modules to avoid ENOTEMPTY errors
    if node_modules.exists():
        try:
            logger.info("Removing existing node_modules to ensure clean install...")
            shutil.rmtree(node_modules, ignore_errors=True)
        except Exception as e:
            logger.warning(f"Failed to remove node_modules: {e}. Continuing with install.")

    def _run_npm_install():
        logger.info("Installing Node dependencies (npm install)...")
        return run_command([npm, "install"], cwd=frontend, timeout=900)

    # First attempt
    result = _run_npm_install()
    if result != 0:
        logger.warning("npm install failed; attempting npm cache clean and retry.")
        # Clean npm cache forcefully
        run_command([npm, "cache", "clean", "--force"], cwd=frontend, timeout=300)
        # Retry install with --force flag
        result = run_command([npm, "install", "--force"], cwd=frontend, timeout=900)
        if result != 0:
            logger.error("npm install failed after retry. Manual intervention may be required.")
            return
    logger.info("[OK] Node dependencies installed")


def restore_dotnet() -> None:
    """Restore .NET packages."""
    dotnet = shutil.which("dotnet")
    if not dotnet:
        logger.error("dotnet SDK not found in PATH. Install .NET SDK 8.0+ and try again.")
        sys.exit(1)

    sln = ROOT / "smas.sln"
    proj = ROOT / "SMAS.API" / "SMAS.API.csproj"

    if sln.exists():
        logger.info("Restoring NuGet packages (solution)...")
        run_command([dotnet, "restore", str(sln)], cwd=ROOT)
    elif proj.exists():
        logger.info("Restoring NuGet packages (project)...")
        run_command([dotnet, "restore", str(proj)], cwd=proj.parent)
    else:
        logger.warning("No .sln or .csproj found; skipping dotnet restore")





def main() -> int:
    """Main setup orchestration (full stack setup)."""
    logger.info("=" * 60)
    logger.info("SMAS Project Setup")
    logger.info("=" * 60)

   
    load_env()

    try:
        # Step 1: Python setup
        logger.info("\n[1/3] Installing Python dependencies globally...")
        install_python_requirements()
        logger.info("[OK] Python dependencies installed")

        # Step 2: Node.js setup
        logger.info("\n[2/3] Setting up frontend dependencies...")
        install_node_dependencies()
        logger.info("[OK] Frontend dependencies ready")

        # Step 3: .NET setup
        logger.info("\n[3/3] Setting up backend dependencies...")
        restore_dotnet()
        logger.info("[OK] Backend dependencies ready")

        logger.info("\n" + "=" * 60)
        logger.info("[OK] Setup complete! Project is ready for development.")
        logger.info("     Note: Database will be created automatically on first run.")
        logger.info("=" * 60)
        return 0

    except Exception as e:
        logger.error(f"Setup failed: {e}")
        return 1


if __name__ == "__main__":
    try:
        exit_code = main()
        sys.exit(exit_code)
    except KeyboardInterrupt:
        logger.info("\nSetup cancelled by user")
        sys.exit(1)
    except Exception as e:
        logger.error(f"Fatal error: {e}")
        sys.exit(1)
