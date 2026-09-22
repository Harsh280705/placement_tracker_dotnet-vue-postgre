<template>
  <div v-if="show" class="dialog-backdrop" @click.self="$emit('cancel')">
    <div class="dialog" role="alertdialog" aria-modal="true" aria-labelledby="delete-title" aria-describedby="delete-desc">
      <h3 id="delete-title">Delete Application?</h3>
      <p id="delete-desc">Are you sure you want to delete <strong>{{ company }}</strong><template v-if="role"> / {{ role }}</template>? This cannot be undone.</p>
      <div class="actions actions-end">
        <button class="btn secondary" @click="$emit('cancel')" :disabled="deleting">Cancel</button>
        <button class="btn danger" :disabled="deleting" @click="$emit('confirm')">
          {{ deleting ? 'Deleting…' : 'Delete' }}
        </button>
      </div>
      <p v-if="error" class="error" role="alert">{{ error }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  show: { type: Boolean, default: false },
  company: { type: String, default: '' },
  role: { type: String, default: '' },
  deleting: { type: Boolean, default: false },
  error: { type: String, default: '' }
});
defineEmits(['confirm', 'cancel']);
</script>
