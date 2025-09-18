<template>
  <q-page class="q-pa-md">
    <div class="row q-mb-md">
      <div class="col">
        <h4 class="q-my-none">{{ $t('navigation.groups.title') }}</h4>
      </div>
      <div class="col-auto">
        <q-btn
          color="primary"
          icon="add"
          :label="$t('groups.create')"
          @click="showCreateDialog = true"
        />
      </div>
    </div>

    <q-separator class="q-mb-md" />

    <!-- Loading state -->
    <div v-if="loading" class="row justify-center q-pa-lg">
      <q-spinner-dots size="50px" color="primary" />
    </div>

    <!-- Groups list -->
    <div v-else-if="groups.length > 0" class="row">
      <div
        v-for="(group, index) in groups"
        :key="group.id || `group-${index}`"
        class="col-12 col-md-6 col-lg-4 q-my-sm"
      >
        <q-card class="q-pa-md cursor-pointer" @click="viewGroupDetails(group)">
          <q-card-section>
            <div class="text-h6">{{ group.name }}</div>
            <div class="text-subtitle2 text-grey-6">
              {{ $t('groups.memberCount', { count: group.memberCount }) }}
            </div>
          </q-card-section>

          <q-card-actions align="right" @click.stop>
          </q-card-actions>
        </q-card>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="row justify-center q-pa-lg">
      <div class="text-center">
        <q-icon name="group" size="4rem" color="grey-5" />
        <div class="text-h6 q-mt-md text-grey-6">{{ $t('groups.empty.title') }}</div>
        <div class="text-body2 text-grey-5 q-mb-md">{{ $t('groups.empty.subtitle') }}</div>
        <q-btn
          color="primary"
          icon="add"
          :label="$t('groups.create')"
          @click="showCreateDialog = true"
        />
      </div>
    </div>

    <!-- Create Group Dialog -->
    <q-dialog v-model="showCreateDialog">
      <q-card style="min-width: 400px">
        <q-card-section>
          <div class="text-h6">{{ $t('groups.createDialog.title') }}</div>
        </q-card-section>

        <q-card-section>
          <q-input
            v-model="newGroupName"
            :label="$t('groups.createDialog.nameLabel')"
            :rules="[(val) => !!val || $t('groups.createDialog.nameRequired')]"
            filled
          />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="$t('common.cancel')" @click="showCreateDialog = false" />
          <q-btn
            color="primary"
            :label="$t('groups.create')"
            :loading="creating"
            @click="createGroup"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import { useRouter } from 'vue-router';
import { Client, type GroupResponse, GroupRequest } from 'src/api/client';

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();

const groups = ref<GroupResponse[]>([]);
const loading = ref(false);
const showCreateDialog = ref(false);
const newGroupName = ref('');
const creating = ref(false);

const client = new Client();

const fetchGroups = async () => {
  loading.value = true;
  try {
    groups.value = await client.getGroups();
  } catch (error) {
    console.error('Error fetching groups:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.fetchFailed'),
    });
  } finally {
    loading.value = false;
  }
};

const createGroup = async () => {
  if (!newGroupName.value.trim()) return;

  creating.value = true;
  try {
    const groupRequest = new GroupRequest({
      name: newGroupName.value.trim(),
    });

    const createdGroup = await client.createGroup(groupRequest);

    console.log('Group created:', createdGroup.id);

    $q.notify({
      type: 'positive',
      message: t('groups.createSuccess'),
    });

    showCreateDialog.value = false;
    newGroupName.value = '';

    // Navigate directly to the newly created group using the ID from response
    if (createdGroup.id) {
      await router.push(`/groups/${createdGroup.id}`);
    } else {
      // Fallback: refresh list and stay on groups page
      await fetchGroups();
    }
  } catch (error) {
    console.error('Error creating group:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.createFailed'),
    });
  } finally {
    creating.value = false;
  }
};


const viewGroupDetails = (group: GroupResponse) => {
  if (group.id) {
    void router.push(`/groups/${group.id}`);
  }
};

onMounted(() => {
  void fetchGroups();
});
</script>
