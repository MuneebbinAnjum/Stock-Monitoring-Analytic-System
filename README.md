<h2>SMAS — Stock Monitoring and Analytics System</h2>

<p>
  SMAS is a full-stack <strong>Enterprise Resource Planning (ERP)</strong> platform built for retail and 
  inventory-driven businesses. It brings together inventory management, sales tracking, order processing, 
  employee/customer management, stock alerts, and demand forecasting into a single dashboard, with real-time 
  updates and role-based access control. Built as a semester project for Object-Oriented Programming and 
  Database Systems at UET Lahore.
</p>

<h3>Modules</h3>
<table>
  <tr><th>Module</th><th>Purpose</th></tr>
  <tr><td><code>SMAS.API</code></td><td>ASP.NET Core 8 Web API — controllers, services, EF Core data access, JWT auth, SignalR hubs</td></tr>
  <tr><td><code>frontend</code></td><td>React + Vite client — dashboard UI consuming the API</td></tr>
  <tr><td><code>SMAS_API.Tests</code></td><td>Backend unit and integration tests</td></tr>
  <tr><td><code>migrations</code></td><td>Database migration scripts (EF Core / SQL)</td></tr>
  <tr><td><code>scripts</code></td><td>Utility and helper scripts</td></tr>
  <tr><td><code>deployment</code></td><td>Deployment configuration for backend/frontend hosting</td></tr>
  <tr><td><code>.github</code></td><td>CI/CD workflow definitions</td></tr>
</table>

<h3>Features</h3>
<ul>
  <li><strong>Inventory Management</strong> — track products, categories, suppliers, stock levels, and automatic reorder-level alerts</li>
  <li><strong>Sales &amp; Order Processing</strong> — full order lifecycle (Pending → Processing → Dispatched → Delivered / Cancelled) with line-item tracking</li>
  <li><strong>Employee &amp; Customer Management</strong> — role-based accounts for Admin, Manager, and Salesman roles, plus customer profiles</li>
  <li><strong>Demand Forecasting</strong> — trend-scored predicted demand per product</li>
  <li><strong>Stock Alerts</strong> — automatic low-stock alerts with resolution tracking</li>
  <li><strong>Real-Time Updates</strong> — live dashboard updates via <a href="https://dotnet.microsoft.com/apps/aspnet/signalr" target="_blank">SignalR</a></li>
  <li><strong>Secure Authentication</strong> — JWT-based auth with issuer/audience/expiry configuration</li>
  <li><strong>Soft Deletes &amp; Auditing</strong> — <code>IsDeleted</code>, <code>CreatedAt</code>, and <code>UpdatedAt</code> tracked on every table</li>
</ul>

<h3>Folder Structure</h3>
<pre><code>SMAS/
├── .github/            # CI/CD workflows
├── deployment/         # Deployment configuration and scripts
├── frontend/            # React + Vite client application
├── migrations/          # Database migration scripts
├── scripts/              # Utility and helper scripts
├── SMAS.API/              # ASP.NET Core Web API (backend)
├── SMAS_API.Tests/         # Backend unit and integration tests
├── .env.example
├── .gitattributes
├── .gitignore
├── Dockerfile
├── netlify.toml
├── run_smas.py          # One-command local dev launcher (build + run + open browser)
├── schema.sql            # Full PostgreSQL schema with seed data
├── setup_all.py          # One-command dependency installer
└── smas.sln
</code></pre>

<h3>Tech Stack</h3>
<ul>
  <li><strong>ASP.NET Core 8</strong> — backend Web API</li>
  <li><strong>React + Vite</strong> — frontend client</li>
  <li><strong>PostgreSQL</strong> — relational database</li>
  <li><strong>Entity Framework Core</strong> — ORM / data access</li>
  <li><strong>SignalR</strong> — real-time updates</li>
  <li><strong>JWT</strong> — authentication (access + refresh tokens)</li>
  <li><strong>Docker</strong> — backend containerization/deployment</li>
  <li><strong>Netlify</strong> (via <code>netlify.toml</code>) — frontend deployment</li>
</ul>

<h3>Prerequisites</h3>
<ul>
  <li><a href="https://dotnet.microsoft.com/download" target="_blank">.NET SDK 8.0+</a></li>
  <li><a href="https://nodejs.org/" target="_blank">Node.js</a> (with npm)</li>
  <li><a href="https://www.postgresql.org/" target="_blank">PostgreSQL</a></li>
  <li>Python 3.9+ (only needed for the setup/run helper scripts)</li>
</ul>

<h3>Getting Started</h3>

<h4>1. Clone the repository</h4>
<pre><code>git clone https://github.com/&lt;your-username&gt;/SMAS.git
cd SMAS
</code></pre>

<h4>2. Configure environment variables</h4>
<pre><code>cp .env.example .env
</code></pre>
<p>Then fill in <code>JWT_KEY</code>, <code>JWT_ISSUER</code>, <code>JWT_AUDIENCE</code>, <code>DB_CONNECTION_STRING</code>, etc.</p>

<h4>3. Install dependencies</h4>
<pre><code>python setup_all.py
</code></pre>
<p>Installs Python packages, npm packages, and restores NuGet packages in one go.</p>

<h4>4. Set up the database</h4>
<pre><code>psql -U postgres -d smas_db -f schema.sql
</code></pre>

<h4>5. Run the app</h4>
<pre><code>python run_smas.py
</code></pre>
<p>Builds and starts both servers, then opens the app in your browser automatically.</p>

<table>
  <tr><th>Service</th><th>URL</th></tr>
  <tr><td>Backend API</td><td><code>http://localhost:5000</code></td></tr>
  <tr><td>Frontend</td><td><code>http://localhost:3000</code></td></tr>
  <tr><td>Health check</td><td><code>http://localhost:5000/health</code></td></tr>
</table>

<h3>Manual Start (Alternative)</h3>

<p><strong>Backend</strong></p>
<pre><code>cd SMAS.API
dotnet run
</code></pre>

<p><strong>Frontend</strong></p>
<pre><code>cd frontend
npm install
npm run dev
</code></pre>

<h3>Running Tests</h3>
<pre><code>dotnet test SMAS_API.Tests
</code></pre>

<h3>Deployment</h3>
<p><strong>Backend (Docker)</strong></p>
<pre><code>docker build -t smas-api .
docker run -p 5000:5000 --env-file .env smas-api
</code></pre>

<p><strong>Frontend (Netlify)</strong></p>
<pre><code>[build]
  base = "frontend"
  publish = "dist"
  command = "npm install && npm run build"
</code></pre>
<p>Connect the repository to Netlify and it will build and deploy automatically from the <code>frontend/</code> directory.</p>

<h3>Database</h3>
<p>
  SMAS uses <strong>PostgreSQL</strong> with a fully relational schema (see <code>schema.sql</code>). Every table 
  follows a consistent auditing pattern — <code>CreatedAt</code>, <code>UpdatedAt</code>, and a soft-delete flag 
  <code>IsDeleted</code> — plus UUID primary keys generated via the <code>pgcrypto</code> extension.
</p>

<table>
  <tr><th>Table</th><th>Purpose</th></tr>
  <tr><td><code>Categories</code></td><td>Product categories (e.g. Electronics, Clothing, Groceries)</td></tr>
  <tr><td><code>Suppliers</code></td><td>Vendor/supplier records — company, contact, and location details</td></tr>
  <tr><td><code>Products</code></td><td>Catalog items — SKU, unit price, stock quantity, and reorder level, linked to a category and supplier</td></tr>
  <tr><td><code>Employees</code></td><td>Staff accounts with role (Admin / Manager / Salesman), hire date, and monthly sales target</td></tr>
  <tr><td><code>Customers</code></td><td>Customer accounts with contact and location info</td></tr>
  <tr><td><code>Orders</code></td><td>Customer orders — status lifecycle, total amount, delivery city, and courier reference</td></tr>
  <tr><td><code>OrderItems</code></td><td>Line items belonging to an order — product, quantity, and unit price at time of sale</td></tr>
  <tr><td><code>SaleRecords</code></td><td>Historical sales transactions per product/employee, used for reporting and forecasting</td></tr>
  <tr><td><code>StockAlerts</code></td><td>Auto-generated low-stock warnings when a product falls below its reorder level</td></tr>
  <tr><td><code>ForecastRecords</code></td><td>Predicted demand and trend score per product for a future date</td></tr>
</table>

<h4>Relationships</h4>
<ul>
  <li><code>Products</code> → <code>Categories</code> (many-to-one, <code>ON DELETE CASCADE</code>)</li>
  <li><code>Products</code> → <code>Suppliers</code> (many-to-one, <code>ON DELETE CASCADE</code>)</li>
  <li><code>Orders</code> → <code>Customers</code> (many-to-one, <code>ON DELETE CASCADE</code>)</li>
  <li><code>Orders</code> → <code>Employees</code> (many-to-one, <code>ON DELETE SET NULL</code> — order is kept even if the employee is removed)</li>
  <li><code>OrderItems</code> → <code>Orders</code> and <code>Products</code> (many-to-one each, <code>ON DELETE CASCADE</code>)</li>
  <li><code>SaleRecords</code> → <code>Products</code> and <code>Employees</code> (many-to-one each, <code>ON DELETE CASCADE</code>)</li>
  <li><code>StockAlerts</code> → <code>Products</code> (many-to-one, <code>ON DELETE CASCADE</code>)</li>
  <li><code>ForecastRecords</code> → <code>Products</code> (many-to-one, <code>ON DELETE CASCADE</code>)</li>
</ul>

<h4>Indexes</h4>
<p>
  Foreign key columns, <code>SKU</code>, <code>Status</code>, <code>SaleDate</code>, <code>IsResolved</code>, and 
  <code>ForecastDate</code> are all indexed to keep common lookups (product-by-category, orders-by-status, 
  sales-by-date, unresolved alerts, upcoming forecasts) fast as the dataset grows.
</p>

<h4>Seed Data</h4>
<p>
  <code>schema.sql</code> ships with sample data out of the box — 3 categories, 5 suppliers, 8 products, 4 
  employees (Admin/Manager/2 Salesmen), 10 customers, 15 orders with line items, 20 historical sale records, 5 
  stock alerts, and 3 forecast records — so the dashboard has realistic data to display immediately after setup.
</p>

<blockquote>
  Note: the subfolders (<code>frontend</code>, <code>SMAS.API</code>, <code>SMAS_API.Tests</code>, 
  <code>migrations</code>, <code>scripts</code>, <code>deployment</code>, <code>.github</code>) were not fully 
  readable at doc-generation time, so exact internal file layouts, routes, and component names above are 
  best-guess. Paste in key files (e.g. <code>Program.cs</code>, controllers, <code>App.jsx</code>) to get this 
  filled in with real details.
</blockquote>
