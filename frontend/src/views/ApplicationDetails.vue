<template>
  <div>
    <router-link to="/" class="back-link">← Back to Dashboard</router-link>
    <p v-if="loading" class="loading">Loading…</p>
    <p v-if="error" class="error" role="alert">{{ error }}</p>
    <div v-if="app" class="card details-card">
      <div class="page-head">
        <h1>Application Details</h1>
        <p class="page-subtitle">{{ app.company }} — {{ app.role }}</p>
      </div>
      <dl class="detail-grid">
        <dt>Company</dt>
        <dd class="cell-strong">{{ app.company }}</dd>
        <dt>Role</dt>
        <dd>{{ app.role }}</dd>
        <dt>Status</dt>
        <dd><span class="badge" :class="badgeClass(app.status)">{{ app.status }}</span></dd>
        <dt>Applied On</dt>
        <dd>{{ app.appliedOn ? String(app.appliedOn).slice(0, 10) : '-' }}</dd>
        <dt v-if="app.jobUrl">Job URL</dt>
        <dd v-if="app.jobUrl"><a :href="app.jobUrl" target="_blank" rel="noopener">{{ app.jobUrl }}</a></dd>
        <dt v-if="app.notes">Notes</dt>
        <dd v-if="app.notes" class="notes-text">{{ app.notes }}</dd>
        <dt>Created At</dt>
        <dd class="muted">{{ app.createdAt }}</dd>
      </dl>
      <div class="actions">
        <router-link to="/" class="btn secondary">Back to Dashboard</router-link>
        <router-link :to="`/applications/${app.id}/edit`" class="btn">Edit</router-link>
        <button class="btn danger" @click="pendingDelete = true">Delete</button>
      </div>
    </div>
    <DeleteConfirmDialog
      :show="pendingDelete"
      :company="app?.company"
      :role="app?.role"
      :deleting="deleting"
      :error="deleteError"
      @confirm="confirmDelete"
      @cancel="pendingDelete = false"
    />
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { getApplication, deleteApplication } from '../services/applicationService.js';
import DeleteConfirmDialog from '../components/DeleteConfirmDialog.vue';

const props = defineProps({ id: { type: [String, Number], required: true } });
const router = useRouter();
const app = ref(null);
const loading = ref(true);
const error = ref('');
const pendingDelete = ref(false);
const deleting = ref(false);
const deleteError = ref('');

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

onMounted(async () => {
  try {
    app.value = await getApplication(props.id);
  } catch (e) {
    error.value = e.status === 404 ? 'Application not found (404).' : (e.message || 'Failed to load.');
  } finally {
    loading.value = false;
  }
});

async function confirmDelete() {
  deleting.value = true;
  try {
    await deleteApplication(props.id);
    router.push('/');
  } catch (e) {
    deleteError.value = e.message || 'Delete failed.';
  } finally {
    deleting.value = false;
  }
}
</script>

<style scoped>
.details-card { max-width: 720px; }
.details-card .page-head { margin-bottom: 6px; }
</style>
