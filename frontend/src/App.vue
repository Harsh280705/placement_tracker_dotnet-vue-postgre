<template>
  <div class="app">
    <header class="topbar">
      <div class="topbar-inner">
        <router-link to="/" class="brand">
          <span class="brand-mark" aria-hidden="true">PT</span>
          <span>Placement Tracker</span>
        </router-link>
        <nav class="topnav" aria-label="Primary">
          <router-link to="/" class="nav-link" active-class="active" exact>Dashboard</router-link>
          <router-link to="/applications/new" class="btn">+ Add Application</router-link>
        </nav>
      </div>
    </header>
    <main class="container">
      <router-view />
    </main>
    <footer class="footer">
      <div class="footer-inner muted">Placement Tracker — track every application in one place.</div>
    </footer>
  </div>
</template>

<style>
.topbar {
  background: #ffffff;
  border-bottom: 1px solid var(--border);
  box-shadow: 0 1px 2px rgba(16, 24, 40, 0.05);
  position: sticky;
  top: 0;
  z-index: 20;
}
.topbar-inner {
  max-width: 1024px;
  margin: 0 auto;
  padding: 12px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.brand {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: var(--text);
  font-weight: 800;
  font-size: 1.1rem;
  text-decoration: none;
  letter-spacing: -0.01em;
}
.brand:hover { color: var(--text); text-decoration: none; }
.brand-mark {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: var(--accent);
  color: #fff;
  font-size: 0.8rem;
  font-weight: 800;
}
.topnav {
  display: flex;
  align-items: center;
  gap: 14px;
}
.nav-link {
  color: var(--text-muted);
  font-weight: 600;
  font-size: 0.92rem;
  padding: 8px 10px;
  border-radius: 7px;
  text-decoration: none;
}
.nav-link:hover { color: var(--text); background: var(--surface-muted); text-decoration: none; }
.nav-link.active { color: var(--accent); background: var(--accent-soft); }
.footer {
  margin-top: auto;
  border-top: 1px solid var(--border);
  background: #fff;
}
.footer-inner {
  max-width: 1024px;
  margin: 0 auto;
  padding: 14px 20px;
  font-size: 0.85rem;
}
@media (max-width: 640px) {
  .topbar-inner { padding: 10px 14px; flex-wrap: wrap; }
  .brand { font-size: 1rem; }
  .topnav { width: 100%; justify-content: space-between; }
  .topnav .btn { flex: 1; }
}
</style>
