<template>
  <div>
    <router-link to="/" class="back-link">← Back to Dashboard</router-link>
    <div class="page-head">
      <h1>Add Application</h1>
      <p class="page-subtitle">Record a new job application you want to track.</p>
    </div>
    <div class="form-card">
      <ApplicationForm submit-label="Add Application" :saving="saving" :server-error="serverError" @submit="create" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import ApplicationForm from '../components/ApplicationForm.vue';
import { createApplication } from '../services/applicationService.js';

const router = useRouter();
const saving = ref(false);
const serverError = ref('');

function extractServerError(e) {
  const details = e.details;
  if (details?.errors) {
    return Object.values(details.errors).flat().join(' ');
  }
  return e.message || 'Create failed.';
}

async function create(payload) {
  saving.value = true;
  serverError.value = '';
  try {
    const created = await createApplication(payload);
    router.push(`/applications/${created.id}`);
  } catch (e) {
    serverError.value = extractServerError(e);
  } finally {
    saving.value = false;
  }
}
</script>
