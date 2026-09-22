<template>
  <div>
    <div class="page-head">
      <h1>Placement Tracker</h1>
      <p class="page-subtitle">Application Dashboard — {{ applications.length }} total application{{ applications.length === 1 ? '' : 's' }}</p>
    </div>

    <p v-if="loading" class="loading">Loading applications…</p>
    <p v-if="error" class="error" role="alert">{{ error }}</p>

    <div v-if="!loading && !error && !applications.length" class="empty-state">
      <strong>No applications yet.</strong>
      <p class="muted">Add your first application to start tracking your placements.</p>
      <router-link to="/applications/new" class="btn" style="margin-top: 10px">+ Add Application</router-link>
    </div>

    <div v-if="applications.length" class="table-wrap">
      <table>
        <thead>
          <tr>
            <th scope="col">Company</th>
            <th scope="col">Role</th>
            <th scope="col">Status</th>
            <th scope="col">Applied On</th>
            <th scope="col">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="app in applications" :key="app.id">
            <td class="cell-strong">{{ app.company }}</td>
            <td>{{ app.role }}</td>
            <td><span class="badge" :class="badgeClass(app.status)">{{ app.status }}</span></td>
            <td>{{ formatDate(app.appliedOn) }}</td>
            <td>
              <div class="actions table-actions">
                <router-link :to="`/applications/${app.id}`" class="link-btn">View</router-link>
                <span class="sep" aria-hidden="true">·</span>
                <router-link :to="`/applications/${app.id}/edit`" class="link-btn">Edit</router-link>
                <span class="sep" aria-hidden="true">·</span>
                <button class="link-btn danger-link" @click="askDelete(app)">Delete</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <DeleteConfirmDialog
      :show="!!pendingDelete"
      :company="pendingDelete?.company"
      :role="pendingDelete?.role"
      :deleting="deleting"
      :error="deleteError"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { getApplications, deleteApplication } from '../services/applicationService.js';
import DeleteConfirmDialog from '../components/DeleteConfirmDialog.vue';

const applications = ref([]);
const loading = ref(true);
const error = ref('');
const pendingDelete = ref(null);
const deleting = ref(false);
const deleteError = ref('');

function formatDate(value) {
  if (!value) return '-';
  return String(value).slice(0, 10);
}

function badgeClass(status) {
  switch (status) {
    case 'Wishlist': return 'badge-wishlist';
    case 'Applied': return 'badge-applied';
    case 'Interview': return 'badge-interview';
    case 'Offer': return 'badge-offer';
    case 'Rejected': return 'badge-rejected';
    default: return 'badge-wishlist';
  }
}

async function load() {
  loading.value = true;
  error.value = '';
  try {
    applications.value = await getApplications();
  } catch (e) {
    error.value = e.message || 'Failed to load applications.';
  } finally {
    loading.value = false;
  }
}

function askDelete(app) {
  pendingDelete.value = app;
  deleteError.value = '';
}

async function confirmDelete() {
  if (!pendingDelete.value) return;
  deleting.value = true;
  try {
    await deleteApplication(pendingDelete.value.id);
    pendingDelete.value = null;
    await load();
  } catch (e) {
    deleteError.value = e.message || 'Delete failed.';
  } finally {
    deleting.value = false;
  }
}

onMounted(load);
</script>

<style scoped>
.table-actions { margin-top: 0; gap: 8px; flex-wrap: nowrap; }
.table-actions .link-btn { font-size: 0.88rem; }
.danger-link { color: var(--danger); }
.danger-link:hover { color: var(--danger-hover); }
.sep { color: var(--border-strong); }
.empty-state strong { color: var(--text); font-size: 1.05rem; }
@media (max-width: 720px) {
  .table-actions { flex-wrap: wrap; }
}
</style>
