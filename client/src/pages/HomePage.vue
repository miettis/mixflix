<template>
  <q-page class="landing-page">
    <!-- Hero Section -->
    <div class="hero-section q-pa-xl">
      <div class="hero-content">
        <div class="row justify-center">
          <div class="col-12 col-md-8 col-lg-6 text-center">
            <h2 class="text-h4 text-weight-bold q-mb-md">{{ $t('landing.slogan') }}</h2>
            <p class="text-body1 text-grey-6 q-mb-xl">
              {{ $t('landing.description') }}
            </p>

            <q-btn
              v-if="!authStore.isAuthenticated"
              color="primary"
              size="md"
              :label="$t('landing.getStarted')"
              icon="img:https://developers.google.com/identity/images/g-logo.png"
              @click="handleGoogleLogin"
              class="q-mb-lg"
              style="min-width: 200px"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Features Section -->
    <div class="features-section q-pa-xl">
      <div class="row justify-center q-mb-xl">
        <div class="col-12 text-center">
          <h2 class="text-h3 text-weight-bold q-mb-md">{{ $t('landing.howItWorks') }}</h2>
          <p class="text-body1 text-grey-6">{{ $t('landing.howItWorksSubtitle') }}</p>
        </div>
      </div>

      <div class="row q-gutter-lg justify-center">
        <!-- Step 1: Create Groups -->
        <div class="col-12 col-md-3">
          <q-card class="feature-card q-pa-lg text-center" flat bordered>
            <q-icon name="group_add" size="3rem" color="primary" class="q-mb-md" />
            <h3 class="text-h5 q-mb-md">{{ $t('landing.step1Title') }}</h3>
            <p class="text-body2 text-grey-6">{{ $t('landing.step1Description') }}</p>
          </q-card>
        </div>

        <!-- Step 2: Rate Content -->
        <div class="col-12 col-md-3">
          <q-card class="feature-card q-pa-lg text-center" flat bordered>
            <q-icon name="star_rate" size="3rem" color="accent" class="q-mb-md" />
            <h3 class="text-h5 q-mb-md">{{ $t('landing.step2Title') }}</h3>
            <p class="text-body2 text-grey-6">{{ $t('landing.step2Description') }}</p>
          </q-card>
        </div>

        <!-- Step 3: Find Perfect Match -->
        <div class="col-12 col-md-3">
          <q-card class="feature-card q-pa-lg text-center" flat bordered>
            <q-icon name="recommend" size="3rem" color="secondary" class="q-mb-md" />
            <h3 class="text-h5 q-mb-md">{{ $t('landing.step3Title') }}</h3>
            <p class="text-body2 text-grey-6">{{ $t('landing.step3Description') }}</p>
          </q-card>
        </div>
      </div>
    </div>

    <!-- Call to Action Section -->
    <div class="cta-section q-pa-xl" v-if="!authStore.isAuthenticated">
      <div class="row justify-center">
        <div class="col-12 col-md-6 text-center">
          <h2 class="text-h4 text-weight-bold q-mb-md">{{ $t('landing.readyToStart') }}</h2>
          <p class="text-body1 text-grey-6 q-mb-lg">{{ $t('landing.readyToStartSubtitle') }}</p>

          <q-btn
            color="primary"
            size="md"
            :label="$t('landing.joinNow')"
            icon="img:https://developers.google.com/identity/images/g-logo.png"
            @click="handleGoogleLogin"
            style="min-width: 200px"
          />
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { useAuthStore } from 'stores/auth-store';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const authStore = useAuthStore();
const $q = useQuasar();
const router = useRouter();

const handleGoogleLogin = async () => {
  try {
    await authStore.loginUserWithGoogle();
    await router.push('/groups');
  } catch (error) {
    console.error('Google login error:', error);
    $q.notify({
      type: 'negative',
      message: t('landing.loginError'),
    });
  }
};
</script>

<style scoped>
.landing-page {
  min-height: 100vh;
}

.hero-section {
  background:
    linear-gradient(rgba(0, 216, 195, 0.8), rgba(154, 0, 216, 0.8)),
    url('~assets/mixflix_hero_2.png');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  color: white;
  min-height: 60vh;
  display: flex;
  align-items: flex-end;
  position: relative;
}

.hero-content {
  width: 100%;
  padding-bottom: 2rem;
}

.hero-content h2 {
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.3);
}

.hero-section h1 {
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
}

.features-section {
  padding: 4rem 1rem;
}

.feature-card {
  height: 100%;
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-5px);
}

.cta-section {
  background: linear-gradient(135deg, #9a00d8, #00d8c3);
  color: white;
}

.cta-section h2 {
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.3);
}
</style>
