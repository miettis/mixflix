import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      { path: '', component: () => import('pages/HomePage.vue') },
      { path: 'groups', component: () => import('pages/GroupList.vue') },
      { path: 'groups/:id', component: () => import('pages/GroupDetails.vue'), name: 'GroupDetails' },
      { path: 'content', component: () => import('pages/ContentList.vue') },
      { path: 'merge-content', component: () => import('pages/MergeContent.vue') },
      { path: 'profile', component: () => import('pages/UserProfile.vue') },
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
