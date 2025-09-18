<template>
  <q-layout view="lHh Lpr lFf">
    <q-header>
      <q-toolbar>
        <q-toolbar-title class="cursor-pointer" @click="router.push('/')">
          <q-img
            src="~assets/mixflix_2_crop.png"
            alt="MixFlix Logo"
            style="width: 152px; height: 100%"
          />
        </q-toolbar-title>

        <!-- Desktop Navigation Menu -->
        <div v-if="authStore.isAuthenticated" class="row q-gutter-sm gt-sm">
          <q-btn
            v-for="link in topBarLinks"
            :key="link.title"
            flat
            :icon="link.icon"
            :label="link.title"
            :to="link.link"
            class="q-px-md"
          />
        </div>

        <!-- Hamburger Menu Button -->
        <q-btn
          v-if="authStore.isAuthenticated"
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleLeftDrawer"
        />
      </q-toolbar>
    </q-header>

    <!-- Sidebar Menu -->
    <q-drawer v-if="authStore.isAuthenticated" v-model="leftDrawerOpen" side="right" bordered>
      <q-list>
        <q-item-label header>&nbsp;</q-item-label>
        <EssentialLink v-for="link in linksList" :key="link.title" v-bind="link" />
        <q-separator />
        <q-item clickable @click="authStore.logoutUser">
          <q-item-section avatar>
            <q-icon name="logout" />
          </q-item-section>

          <q-item-section>
            <q-item-label>{{ t('navigation.logout.title') }}</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <div class="page-wrapper">
        <router-view />
      </div>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import EssentialLink, { type EssentialLinkProps } from 'components/EssentialLink.vue';
import { useAuthStore } from 'stores/auth-store';
import { onMounted, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { Client, type UserResponse } from 'src/api/client';

const route = useRoute();
const authStore = useAuthStore();
const { t } = useI18n();
const client = new Client();
const router = useRouter();

const checkAuthAndRedirect = () => {
  if (authStore.authLoading) return; // Don't check if still loading

  if (!authStore.isAuthenticated) {
    console.log('User is not authenticated, redirecting to login.');
    if (route.path !== '/' && route.name !== 'GroupDetails') {
      void router.replace('/'); // Redirect to home or login page
    }
  } else {
    void client.ensureUser().then(() => {
      // Fetch and save user profile after ensuring user exists
      void client.getProfile().then((profile: UserResponse) => {
        authStore.setUserProfile(profile);
      }).catch((error: unknown) => {
        console.error('Failed to fetch user profile:', error);
      });
    }).catch((error: unknown) => {
      console.error('Failed to ensure user:', error);
    });
  }
};

onMounted(() => {
  console.log('Component mounted. Current route:', route.path);

  // Check auth state once loading is complete
  checkAuthAndRedirect();
});

// Watch for auth loading to complete
watch(
  () => authStore.authLoading,
  (isLoading) => {
    if (!isLoading) {
      checkAuthAndRedirect();
    }
  },
  { immediate: true },
);

watch(
  () => route.path,
  () => {
    console.log('Navigated. Current route:', route.path);
    // Only check auth state if not loading
    if (!authStore.authLoading && !authStore.isAuthenticated) {
      if (route.path !== '/' && route.name !== 'GroupDetails') {
        console.log('User is not authenticated, redirecting to login.', route.name);
        void router.replace('/');
      }
    }
  },
);

const linksList = computed((): EssentialLinkProps[] => [
  {
    title: t('navigation.groups.title'),
    caption: t('navigation.groups.caption'),
    icon: 'group',
    link: '/groups',
  },
  {
    title: t('navigation.content.title'),
    caption: t('navigation.content.caption'),
    icon: 'play_circle',
    link: '/content',
  },
  ...(authStore.userProfile?.isAdmin ? [{
    title: t('navigation.mergeContent.title'),
    caption: t('navigation.mergeContent.caption'),
    icon: 'merge',
    link: '/merge-content',
  }] : []),
  {
    title: t('navigation.profile.title'),
    caption: t('navigation.profile.caption'),
    icon: 'person',
    link: '/profile',
  },
]);

const topBarLinks = computed((): EssentialLinkProps[] => [
  {
    title: t('navigation.groups.title'),
    caption: t('navigation.groups.caption'),
    icon: 'group',
    link: '/groups',
  },
  {
    title: t('navigation.content.title'),
    caption: t('navigation.content.caption'),
    icon: 'play_circle',
    link: '/content',
  },
  ...(authStore.userProfile?.isAdmin ? [{
    title: t('navigation.mergeContent.title'),
    caption: t('navigation.mergeContent.caption'),
    icon: 'merge',
    link: '/merge-content',
  }] : []),
]);

const leftDrawerOpen = ref(false);

function toggleLeftDrawer() {
  leftDrawerOpen.value = !leftDrawerOpen.value;
}
</script>

<style scoped>
.page-wrapper {
  max-width: 1200px;
  margin: 0 auto;
}

:deep(.q-toolbar) {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 16px;
}

/* Make active/highlighted menu items more visible */
:deep(.q-btn.router-link-active) {
  background-color: rgba(0, 216, 195, 0.2) !important;
  color: #00d8c3 !important;
}

:deep(.q-item.router-link-active) {
  background-color: rgba(0, 216, 195, 0.15) !important;
  color: #00d8c3 !important;
}

:deep(.q-item.router-link-active .q-icon) {
  color: #00d8c3 !important;
}

@media (max-width: 768px) {
  :deep(.q-toolbar) {
    padding: 0 8px;
  }
}
</style>
