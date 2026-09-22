import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';
import AddApplication from '../views/AddApplication.vue';
import ApplicationDetails from '../views/ApplicationDetails.vue';
import EditApplication from '../views/EditApplication.vue';

const routes = [
  { path: '/', name: 'dashboard', component: Dashboard },
  { path: '/applications/new', name: 'add', component: AddApplication },
  { path: '/applications/:id', name: 'details', component: ApplicationDetails, props: true },
  { path: '/applications/:id/edit', name: 'edit', component: EditApplication, props: true }
];

export default createRouter({
  history: createWebHistory(),
  routes
});
