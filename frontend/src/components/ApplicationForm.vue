<template>
  <form @submit.prevent="onSubmit" novalidate>
    <div class="form-row-2">
      <div class="form-group">
        <label for="company">Company</label>
        <input id="company" v-model="form.company" type="text" placeholder="e.g. Acme Labs" autocomplete="organization" />
        <span v-if="errors.company" class="error" role="alert">{{ errors.company }}</span>
      </div>
      <div class="form-group">
        <label for="role">Role</label>
        <input id="role" v-model="form.role" type="text" placeholder="e.g. Backend Intern" autocomplete="off" />
        <span v-if="errors.role" class="error" role="alert">{{ errors.role }}</span>
      </div>
    </div>
    <div class="form-row-2">
      <div class="form-group">
        <label for="status">Status</label>
        <select id="status" v-model="form.status">
          <option v-for="s in allowedStatuses" :key="s" :value="s">{{ s }}</option>
        </select>
        <span v-if="errors.status" class="error" role="alert">{{ errors.status }}</span>
      </div>
      <div class="form-group">
        <label for="appliedOn">Applied On</label>
        <input id="appliedOn" v-model="form.appliedOn" type="date" />
        <span v-if="errors.appliedOn" class="error" role="alert">{{ errors.appliedOn }}</span>
      </div>
    </div>
    <div class="form-group">
      <label for="jobUrl">Job URL</label>
      <input id="jobUrl" v-model="form.jobUrl" type="url" placeholder="https://example.com/job" inputmode="url" />
      <span v-if="errors.jobUrl" class="error" role="alert">{{ errors.jobUrl }}</span>
    </div>
    <div class="form-group">
      <label for="notes">Notes</label>
      <textarea id="notes" v-model="form.notes" rows="4" maxlength="1000" placeholder="Optional notes (max 1000 chars)"></textarea>
      <span v-if="errors.notes" class="error" role="alert">{{ errors.notes }}</span>
    </div>
    <div v-if="serverError" class="error" role="alert">{{ serverError }}</div>
    <div class="actions actions-end">
      <router-link to="/" class="btn secondary">Cancel</router-link>
      <button type="submit" class="btn" :disabled="saving">{{ saving ? 'Saving…' : submitLabel }}</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, watch } from 'vue';
import { allowedStatuses } from '../services/applicationService.js';

const props = defineProps({
  initial: { type: Object, default: () => ({}) },
  submitLabel: { type: String, default: 'Save' },
  saving: { type: Boolean, default: false },
  serverError: { type: String, default: '' }
});
const emit = defineEmits(['submit']);

const form = reactive({
  company: '',
  role: '',
  status: 'Applied',
  appliedOn: '',
  jobUrl: '',
  notes: ''
});
const errors = reactive({});

watch(() => props.initial, (val) => {
  if (val) {
    form.company = val.company ?? '';
    form.role = val.role ?? '';
    form.status = val.status ?? 'Applied';
    form.appliedOn = val.appliedOn ? String(val.appliedOn).slice(0, 10) : '';
    form.jobUrl = val.jobUrl ?? '';
    form.notes = val.notes ?? '';
  }
}, { immediate: true });

function validate() {
  Object.keys(errors).forEach(k => delete errors[k]);
  if (!form.company || form.company.trim().length < 2 || form.company.trim().length > 100) {
    errors.company = 'Company must be 2-100 characters.';
  }
  if (!form.role || form.role.trim().length < 2 || form.role.trim().length > 100) {
    errors.role = 'Role must be 2-100 characters.';
  }
  if (!allowedStatuses.includes(form.status)) {
    errors.status = 'Status must be one of: Wishlist, Applied, Interview, Offer, Rejected.';
  }
  if (form.status !== 'Wishlist' && !form.appliedOn) {
    errors.appliedOn = 'Applied date is required unless status is Wishlist.';
  }
  if (form.jobUrl) {
    try { new URL(form.jobUrl); } catch { errors.jobUrl = 'Please enter a valid URL (e.g. https://example.com/job).'; }
  }
  if (form.notes && form.notes.length > 1000) {
    errors.notes = 'Notes cannot be longer than 1000 characters.';
  }
  return Object.keys(errors).length === 0;
}

function onSubmit() {
  if (!validate()) return;
  emit('submit', {
    company: form.company.trim(),
    role: form.role.trim(),
    status: form.status,
    appliedOn: form.appliedOn || null,
    jobUrl: form.jobUrl || null,
    notes: form.notes || null
  });
}
</script>
