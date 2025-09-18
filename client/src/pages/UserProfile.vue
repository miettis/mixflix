<template>
  <q-page class="q-pa-md">
    <!-- Loading state -->
    <div v-if="loading" class="row justify-center q-pa-lg">
      <q-spinner-dots size="50px" color="primary" />
    </div>

    <!-- Profile form -->
    <div v-else class="row justify-center">
      <div class="col-12 col-md-8 col-lg-6">
        <q-card class="q-pa-md">
          <q-card-section>
            <div class="text-h5 q-mb-md text-center">{{ $t('profile.title') }}</div>

            <q-form @submit="updateProfile" class="q-gutter-md">
              <q-input
                v-model="profileForm.name"
                :label="$t('profile.nameLabel')"
                :rules="[(val) => !!val || $t('profile.nameRequired')]"
                filled
                class="q-mb-md"
              />
              <q-input
                :model-value="profile?.email || ''"
                :label="$t('profile.emailLabel')"
                readonly
                filled
                class="q-mb-md"
              />

              <div class="row q-gutter-sm">
                <q-btn
                  type="submit"
                  color="primary"
                  :label="$t('profile.save')"
                  :loading="saving"
                  class="col"
                />
                <q-btn flat :label="$t('common.cancel')" @click="resetForm" class="col" />
              </div>
            </q-form>

            <!-- Account Deletion Section -->
            <q-separator class="q-my-lg" />
            <div class="text-subtitle2 q-mb-sm text-negative">{{ $t('profile.dangerZone') }}</div>
            <div class="text-body2 text-grey-6 q-mb-md">
              {{ $t('profile.deleteAccountWarning') }}
            </div>
            <q-btn
              color="negative"
              outline
              :label="$t('profile.deleteAccount')"
              icon="delete_forever"
              @click="confirmDeleteAccount"
              :loading="deleting"
            />
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Error state -->
    <div v-if="error" class="row justify-center q-pa-lg">
      <div class="text-center">
        <q-icon name="error" size="4rem" color="negative" />
        <div class="text-h6 q-mt-md text-negative">{{ $t('profile.errors.loadFailed') }}</div>
        <q-btn color="primary" :label="$t('profile.retry')" @click="fetchProfile" class="q-mt-md" />
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth-store';
import { Client, type UserResponse, UserRequest } from 'src/api/client';

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();
const authStore = useAuthStore();

const profile = ref<UserResponse | null>(null);
const loading = ref(false);
const saving = ref(false);
const deleting = ref(false);
const error = ref(false);

const profileForm = ref({
  name: '',
});

const client = new Client();

const fetchProfile = async () => {
  loading.value = true;
  error.value = false;
  try {
    profile.value = await client.getProfile();

    // Initialize form with current profile data
    profileForm.value.name = profile.value?.name || '';
  } catch (err) {
    console.error('Error fetching profile:', err);
    error.value = true;
    $q.notify({
      type: 'negative',
      message: t('profile.errors.loadFailed'),
    });
  } finally {
    loading.value = false;
  }
};

const updateProfile = async () => {
  if (!profileForm.value.name.trim()) return;

  saving.value = true;
  try {
    const profileRequest = new UserRequest({
      name: profileForm.value.name.trim(),
    });

    await client.updateProfile(profileRequest);

    // Update local profile data with the new name
    if (profile.value) {
      profile.value.name = profileForm.value.name.trim();
    }

    $q.notify({
      type: 'positive',
      message: t('profile.updateSuccess'),
    });
  } catch (err) {
    console.error('Error updating profile:', err);
    $q.notify({
      type: 'negative',
      message: t('profile.errors.updateFailed'),
    });
  } finally {
    saving.value = false;
  }
};

const resetForm = () => {
  // Reset form to original profile data
  profileForm.value.name = profile.value?.name || '';
};

const confirmDeleteAccount = () => {
  $q.dialog({
    title: t('profile.deleteDialog.title'),
    message: t('profile.deleteDialog.message'),
    persistent: true,
    ok: {
      label: t('profile.deleteDialog.confirm'),
      color: 'negative',
    },
    cancel: {
      label: t('common.cancel'),
      flat: true,
    },
  }).onOk(() => {
    void deleteAccount();
  });
};

const deleteAccount = async () => {
  deleting.value = true;
  try {
    await client.deleteProfile();

    $q.notify({
      type: 'positive',
      message: t('profile.deleteSuccess'),
    });

    // Log out the user and redirect to home
    await authStore.logoutUser();
    await router.push('/');
  } catch (err) {
    console.error('Error deleting account:', err);
    $q.notify({
      type: 'negative',
      message: t('profile.errors.deleteFailed'),
    });
  } finally {
    deleting.value = false;
  }
};

onMounted(() => {
  void fetchProfile();
});
</script>

<style scoped>
.q-card {
  max-width: 500px;
  margin: 0 auto;
}
</style>
