<template>
  <div>
    <router-link to="/" class="back-link">← Back to Dashboard</router-link>
    <div class="page-head">
      <h1>Edit Application</h1>
      <p class="page-subtitle">Update the details of this application.</p>
    </div>
    <p v-if="loading" class="loading">Loading…</p>
    <p v-if="loadError" class="error" role="alert">{{ loadError }}</p>
    <div v-if="initial" class="form-card">
      <ApplicationForm
        :initial="initial"
        submit-label="Save Changes"
        :saving="saving"
        :server-error="serverError"
        @submit="save"
      />
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import ApplicationForm from '../components/ApplicationForm.vue';
import { getApplication, updateApplication } from '../services/applicationService.js';

const props = defineProps({ id: { type: [String, Number], required: true } });
const router = useRouter();
const initial = ref(null);
const loading = ref(true);
const loadError = ref('');
const saving = ref(false);
const serverError = ref('');

onMounted(async () => {
  try {
    initial.value = await getApplication(props.id);
  } catch (e) {
    loadError.value = e.status === 404 ? 'Application not found (404).' : (e.message || 'Failed to load.');
  } finally {
    loading.value = false;
  }
});

async function save(payload) {
  saving.value = true;
  serverError.value = '';
  try {
    await updateApplication(props.id, payload);
    router.push(`/applications/${props.id}`);
  } catch (e) {
    const details = e.details;
    serverError.value = details?.errors
      ? Object.values(details.errors).flat().join(' ')
      : (e.message || 'Update failed.');
  } finally {
    saving.value = false;
  }
}
</script>
