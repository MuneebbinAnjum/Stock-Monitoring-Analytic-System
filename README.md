<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>SMAS — Stock Monitoring and Analytic System</title>
<style>
  :root {
    --bg: #0d1117;
    --bg-secondary: #161b22;
    --border: #30363d;
    --text: #c9d1d9;
    --text-secondary: #8b949e;
    --link: #58a6ff;
    --accent: #3fb950;
    --warn: #d29922;
    --danger: #f85149;
    --code-bg: #161b22;
  }
  body {
    background: var(--bg);
    color: var(--text);
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
    max-width: 900px;
    margin: 0 auto;
    padding: 40px 20px 80px;
    line-height: 1.6;
  }
  h1, h2, h3 { color: #fff; border-bottom: 1px solid var(--border); padding-bottom: 8px; }
  h1 { font-size: 2em; }
  h2 { font-size: 1.5em; margin-top: 40px; }
  h3 { font-size: 1.2em; border-bottom: none; margin-top: 28px; }
  a { color: var(--link); text-decoration: none; }
  a:hover { text-decoration: underline; }
  code {
    background: var(--code-bg);
    border: 1px solid var(--border);
    border-radius: 4px;
    padding: 2px 6px;
    font-family: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, monospace;
    font-size: 0.88em;
  }
  pre {
    background: var(--code-bg);
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 16px;
    overflow-x: auto;
  }
  pre code { border: none; padding: 0; background: none; }
  table { border-collapse: collapse; width: 100%; margin: 16px 0; }
  th, td { border: 1px solid var(--border); padding: 8px 12px; text-align: left; }
  th { background: var(--bg-secondary); color: #fff; }
  tr:nth-child(even) { background: var(--bg-secondary); }
  .badges { margin: 10px 0 20px; }
  .badges img { margin-right: 6px; }
  .center { text-align: center; }
  ul, ol { padding-left: 24px; }
  .callout {
    background: var(--bg-secondary);
    border-left: 4px solid var(--accent);
    padding: 10px 16px;
    border-radius: 4px;
    margin: 16px 0;
  }
  .callout.warn { border-left-color: var(--warn); }
  .tree {
    background: var(--code-bg);
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 16px;
    font-family: monospace;
    white-space: pre;
    overflow-x: auto;
    font-size: 0.85em;
  }
  hr { border: none; border-top: 1px solid var(--border); margin: 32px 0; }
</style>
</head>
<body>

<h1 class="center">📦 SMAS — Stock Monitoring and Analytic System</h1>
<p class="center">A web-based ERP platform that brings inventory, orders, employees, finance, and real-time analytics into one system for small and medium-sized retail businesses.</p>

<div class="badges center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&amp;logoColor=white" alt=".NET 8">
  <img src="https://img.shields.io/badge/React-Vite-61DAFB?logo=react&amp;logoColor=black" alt="React">
  <img src="https://img.shields.io/badge/PostgreSQL-23_tables-4169E1?logo=postgresql&amp;logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/EF_Core-Code_First-512BD4" alt="EF Core">
  <img src="https://img.shields.io/badge/SignalR-Realtime-1E90FF" alt="SignalR">
  <img src="https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens" alt="JWT">
  <img src="https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&amp;logoColor=white" alt="Docker">
</div>

<hr>

<h2>📖 About</h2>
<p>Built as a semester ERP project to replace the manual, error-prone inventory practices that small and medium-sized retailers across Pakistan — toy shops, clothing boutiques, grocery outlets, electronics retailers — still run on: physical registers, disconnected spreadsheets, and gut-feeling employee reviews. <b>SMAS</b> is a three-layer, web-based ERP covering inventory, order management, employee operations, financial accounting, and real-time analytics in one integrated system.</p>

<div class="callout">
Code-First with EF Core: the C# model classes are the source of truth, and EF Core generates the full PostgreSQL schema (23 tables) from them.
</div>

<h2>🎯 Problem Statement</h2>
<p>Stock levels tracked through physical registers or simple spreadsheets; sales data neither consolidated nor analyzed; employee performance judged entirely on subjective grounds. This absence of a unified digital system compounds into locked-up capital in overstock, eroded customer trust during stockouts, un-forecasted demand, and no mechanism for local storefronts to reach buyers beyond their immediate vicinity.</p>

<h2>🏗️ Architecture</h2>
<p>SMAS follows a strict three-layer architecture:</p>
<div class="tree">┌─────────────────────────────────────────┐
│  Layer 1 — Presentation                   │
│  React + Vite (HTML/CSS/JS)               │
│  Buttons, screens, tables, charts         │
└───────────────────┬───────────────────────┘
                     │  REST + SignalR
┌───────────────────▼───────────────────────┐
│  Layer 2 — Business Logic                 │
│  ASP.NET Core 8 + C#                      │
│  Controllers → Services → Repositories    │
│  Tax calc, discounts, permission checks   │
└───────────────────┬───────────────────────┘
                     │  EF Core (Code-First)
┌───────────────────▼───────────────────────┐
│  Layer 3 — Data                           │
│  PostgreSQL — 23 tables, 3NF              │
│  UUID PKs, timestamps, soft-delete        │
└─────────────────────────────────────────┘</div>

<h3>Backend layout</h3>
<ul>
  <li><b>Controllers</b> — receive HTTP requests, no business logic (<code>AuthController</code>, <code>OrdersController</code>, <code>ProductsController</code>).</li>
  <li><b>Services</b> — core business logic: validation, calculations, processing (<code>OrderService</code>, <code>AuthService</code>, <code>ReportService</code>).</li>
  <li><b>Repositories</b> — CRUD via EF Core, connect services to the database (<code>ProductRepository</code>, <code>OrderRepository</code>).</li>
  <li><b>Models</b> — C# classes representing database entities (<code>Product</code>, <code>Order</code>, <code>Employee</code>, <code>Customer</code>).</li>
  <li><b>DTOs</b> — carry only the required data between backend and frontend (<code>LoginDto</code>, <code>CreateOrderDto</code>, <code>ProductResponseDto</code>).</li>
  <li><b>Middleware</b> — cross-cutting concerns like auth, logging, error handling (<code>JwtMiddleware</code>, <code>GlobalExceptionMiddleware</code>).</li>
</ul>

<h2>✨ Feature Coverage</h2>
<table>
  <tr><th>Area</th><th>What's implemented</th></tr>
  <tr><td><b>Inventory</b></td><td>Real-time stock levels, add/edit products, automatic reorder alerts.</td></tr>
  <tr><td><b>Orders</b></td><td>Full lifecycle — Pending → Processing → Dispatched → Delivered (or Cancelled) — with live tracking.</td></tr>
  <tr><td><b>Employees</b></td><td>Roles, salaries, commissions, approval workflows, performance tracking.</td></tr>
  <tr><td><b>Customer portal</b></td><td>Browsing, cart, wishlist, order tracking, complaint filing.</td></tr>
  <tr><td><b>Finance</b></td><td>Tax calculations, discount management, commission tracking, salary summaries.</td></tr>
  <tr><td><b>Dashboard</b></td><td>Live charts — revenue trends, sales by city, product performance.</td></tr>
  <tr><td><b>Notifications</b></td><td>Real-time alerts for orders, complaints, and stock levels via SignalR.</td></tr>
  <tr><td><b>Access control</b></td><td>Role-based — Admin, Salesman, and Buyer each see only what they need.</td></tr>
</table>

<h3>Who it's for</h3>
<table>
  <tr><th>Role</th><th>How SMAS helps them</th></tr>
  <tr><td>Admin</td><td>Full control — products, employees, orders, reports, settings, complaints. Live dashboard with revenue trends.</td></tr>
  <tr><td>Salesman</td><td>Track personal sales, view commissions earned, process walk-in customer orders.</td></tr>
  <tr><td>Buyer / Customer</td><td>Browse products, manage cart and wishlist, place online orders, track deliveries, file complaints.</td></tr>
</table>

<h2>🗄️ Database Design</h2>
<p>PostgreSQL, 23 tables, Third Normal Form (3NF). Every table has a UUID primary key (<code>gen_random_uuid()</code>), <code>CreatedAt</code>/<code>UpdatedAt</code> timestamps, and an <code>IsDeleted</code> soft-delete flag — all conceptually inherited from a common base <code>Entity</code>.</p>

<table>
  <tr><th>Module</th><th>Covers</th></tr>
  <tr><td>Core Commerce</td><td>Products, Categories, Orders, Order Items — the backbone of buying and selling.</td></tr>
  <tr><td>User Management</td><td>Customers, Employees, authentication, roles, approval status, refresh tokens.</td></tr>
  <tr><td>Inventory &amp; Supply Chain</td><td>Suppliers, stock movements, low-stock alerts, product sourcing.</td></tr>
  <tr><td>Pricing, Sales &amp; Forecasting</td><td>Discounts, commissions, sale records, demand forecasting.</td></tr>
  <tr><td>Customer Interaction</td><td>Complaints, complaint messages, cart, wishlist, notifications.</td></tr>
  <tr><td>System &amp; Logging</td><td>Audit logs, system settings — auditability and traceability.</td></tr>
</table>

<h3>Key relationships</h3>
<ul>
  <li>Category → Products (1:N), Category → sub-Categories (1:N self-referencing)</li>
  <li>Supplier → Products (1:N, product retained if supplier deleted)</li>
  <li>Product → Order Items, Discounts, Product Images (1:N)</li>
  <li>Customer → Orders, Cart Items (1:N)</li>
  <li>Employee → Orders, Sale Records, Commissions, Notifications (1:N, sales history retained on employee deletion)</li>
  <li>Order → Order Items, Complaints (1:N)</li>
  <li>Complaint → Complaint Messages (1:N, chat thread)</li>
</ul>

<div class="callout">
The full schema — including seed data for categories, suppliers, products, employees, customers, orders, and sale records — lives in <code>schema.sql</code>.
</div>

<h2>🧱 OOP Design</h2>
<ul>
  <li><b>Classes:</b> separate model classes per domain entity — <code>Product</code>, <code>Order</code>, <code>Employee</code>, etc.</li>
  <li><b>Inheritance:</b> all 17 models inherit from a common base <code>Entity</code> class, adding their own attributes on top of shared ones (Id, timestamps, soft-delete).</li>
  <li><b>Encapsulation:</b> internal state hidden behind private fields with controlled public get/set properties.</li>
  <li><b>Polymorphism:</b> shared base behavior overridden per entity across models and services.</li>
  <li><b>Abstraction:</b> the base <code>Entity</code> class is abstracted — its attributes are inherited across all other models rather than duplicated.</li>
</ul>

<h2>🛠️ Tech Stack</h2>
<p>ASP.NET Core 8, C#, Entity Framework Core (Code-First), PostgreSQL, React, Vite, SignalR, JWT (access + refresh tokens), Docker, Netlify.</p>

<h2>📦 Installation</h2>

<h3>Prerequisites</h3>
<ul>
  <li><a href="https://dotnet.microsoft.com/download/dotnet/8.0">.NET SDK 8.0+</a></li>
  <li><a href="https://nodejs.org/">Node.js</a> + npm</li>
  <li><a href="https://www.postgresql.org/">PostgreSQL</a></li>
  <li>Python 3 (for the setup/run helper scripts)</li>
</ul>

<h3>1. Clone the repo</h3>
<pre><code>git clone https://github.com/MuneebbinAnjum/&lt;repo-name&gt;.git
cd SMAS</code></pre>

<h3>2. Configure environment variables</h3>
<pre><code>cp .env.example .env</code></pre>
<pre><code>JWT_KEY=your-very-long-secret-key-at-least-64-characters-long
JWT_ISSUER=SMAS
JWT_AUDIENCE=SMASUsers
JWT_EXPIRY_MINUTES=60
JWT_REFRESH_EXPIRY_DAYS=7

DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=smas_db;Username=postgres;Password=your-password;SSL Mode=Disable</code></pre>

<h3>3. Install dependencies</h3>
<pre><code>python setup_all.py</code></pre>
<p>Restores NuGet packages, installs frontend npm dependencies, and installs any Python requirements — all in one go.</p>

<h2>▶️ Usage</h2>
<pre><code>python run_smas.py</code></pre>
<p>Builds the backend, installs + builds the frontend, starts both, waits for health checks, and opens the app in your browser. The database itself needs no manual step — EF Core creates and migrates it automatically on first run.</p>

<div class="callout">
<b>Default ports:</b> API on <code>http://localhost:5000</code> (health check at <code>/health</code>), frontend on <code>http://localhost:3000</code>. Override with <code>SMAS_API_HOST</code>, <code>SMAS_API_PORT</code>, <code>SMAS_FRONTEND_HOST</code>, <code>SMAS_FRONTEND_PORT</code>.
</div>

<h3>Manual setup (alternative)</h3>
<pre><code># Backend
cd SMAS.API
dotnet restore
dotnet run

# Frontend (separate terminal)
cd frontend
npm install
npm run dev</code></pre>

<h3>Docker (backend only)</h3>
<pre><code>docker build -t smas-api .
docker run -p 5000:5000 --env-file .env smas-api</code></pre>

<h2>📁 Project Structure</h2>
<div class="tree">SMAS/
├── SMAS.API/            # ASP.NET Core 8 backend (Controllers, Services, Repositories, Models, DTOs, Middleware)
├── SMAS_API.Tests/      # Backend test project
├── frontend/            # React + Vite frontend
├── migrations/          # EF Core migrations
├── config/              # Configuration files
├── deployment/          # Deployment configs
├── scripts/             # Utility scripts
├── src/                 # Additional source
├── schema.sql           # Full PostgreSQL schema + seed data
├── setup_all.py         # One-shot dependency installer (Python venv, npm, dotnet restore)
├── run_smas.py          # Builds backend, builds & serves frontend, opens browser
├── Dockerfile            # Backend container build (ASP.NET Core 8)
├── netlify.toml          # Frontend deployment config (Netlify)
├── .env.example           # Environment variable template
└── smas.sln               # Visual Studio solution file</div>

<h2>☁️ Deployment</h2>
<p>Deployed as a split static frontend + backend, matching the two very different runtime needs.</p>

<h3>1. Frontend (Netlify)</h3>
<p>Build command <code>npm install && npm run build</code>, publish directory <code>dist</code>, with SPA fallback routing configured in <code>netlify.toml</code>.</p>

<h3>2. Backend (Docker)</h3>
<p>Multi-stage <code>Dockerfile</code> — builds on the .NET 8 SDK image, runs on the ASP.NET Core 8 runtime image. Deployable to any container host (Render, Railway, Azure, etc.).</p>

<h2>🧪 Testing</h2>
<p>Backend tests live in <code>SMAS_API.Tests</code>.</p>

<h2>📄 Documentation</h2>
<p>A full project manual (problem definition, architecture, ER diagrams, OOP breakdown, database design) is included — see <code>SMAS_Manual.docx</code>.</p>

<h2>🔐 Security Notes</h2>
<ul>
  <li>Never commit your real <code>.env</code> — only <code>.env.example</code> is tracked.</li>
  <li>Passwords are stored as hashes (bcrypt-style), never in plaintext.</li>
  <li>Auth uses short-lived JWT access tokens plus long-lived refresh tokens.</li>
</ul>

<hr>
<p class="center" style="color: var(--text-secondary);">Built by <a href="https://github.com/MuneebbinAnjum">Muneeb bin Anjum</a> — Data Science student, UET Lahore.</p>

</body>
</html>
