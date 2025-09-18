<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ $t('mergeContent.title') }}</div>
    <div class="text-body1 text-grey-6 q-mb-lg">{{ $t('mergeContent.description') }}</div>

    <!-- Loading state -->
    <div v-if="loading" class="row justify-center q-pa-lg">
      <q-spinner-dots size="50px" color="primary" />
    </div>

    <!-- No duplicates found -->
    <div v-else-if="duplicates.length === 0 && !loading" class="text-center q-pa-lg">
      <q-icon name="check_circle" size="4rem" color="positive" />
      <div class="text-h6 q-mt-md text-positive">{{ $t('mergeContent.noDuplicates.title') }}</div>
      <div class="text-body2 text-grey-6">{{ $t('mergeContent.noDuplicates.description') }}</div>
    </div>

    <!-- Current duplicate group -->
    <div v-else-if="currentDuplicate" class="q-mb-lg">
      <q-card class="q-pa-md">
        <q-card-section>
          <div class="text-h6 q-mb-xs">
            {{ currentDuplicate.title }}
            <span v-if="currentDuplicate.releaseYear" class="text-grey-6">({{ currentDuplicate.releaseYear }})</span>
          </div>
          <div class="text-body2 text-grey-6 q-mb-md">
            {{ $t('content.contentTypes.' + (currentDuplicate.type === 1 ? 'movie' : 'series')) }}
          </div>
          <div class="text-body2 text-grey-7 q-mb-lg">
            {{ $t('mergeContent.foundDuplicates', { count: currentDuplicate.items?.length || 0 }) }}
          </div>

          <!-- Items comparison -->
          <div class="row q-gutter-md">
            <div
              v-for="(item, index) in currentDuplicate.items"
              :key="item.id || `item-${index}`"
              class="col-12 col-md-6 col-lg-4"
            >
              <q-card
                :class="{
                  'bg-primary text-white': selectedItemIds.includes(item.id!),
                  'bg-secondary text-white': selectedDescriptionId === item.id && !selectedItemIds.includes(item.id!),
                  'bg-grey-2': !selectedItemIds.includes(item.id!) && selectedDescriptionId !== item.id
                }"
                class="cursor-pointer transition-all"
                @click="toggleItemSelection(item.id!)"
              >
                <q-card-section>
                  <div class="text-subtitle2 q-mb-xs">
                    {{ $t('mergeContent.item') }} {{ index + 1 }}
                    <q-chip
                      v-if="selectedItemIds.includes(item.id!)"
                      dense
                      color="white"
                      text-color="primary"
                      :label="$t('mergeContent.selected')"
                      class="q-ml-sm"
                    />
                  </div>

                  <!-- Image -->
                  <div v-if="item.imageUrl" class="q-mb-md">
                    <q-img
                      :src="item.imageUrl"
                      :alt="currentDuplicate.title"
                      style="max-width: 200px; max-height: 300px"
                      class="rounded-borders"
                      loading="lazy"
                    >
                      <template v-slot:error>
                        <div class="absolute-full flex flex-center bg-grey-3">
                          <q-icon name="broken_image" size="30px" color="grey-6" />
                        </div>
                      </template>
                    </q-img>
                  </div>

                  <!-- Description -->
                  <div v-if="item.shortDescription" class="q-mb-md">
                    <div class="text-caption text-weight-medium q-mb-xs">
                      {{ $t('content.dialog.shortDescription') }}
                      <q-btn
                        v-if="selectedDescriptionId !== item.id"
                        flat
                        dense
                        size="sm"
                        color="accent"
                        :label="$t('mergeContent.useDescription')"
                        @click.stop="selectDescription(item.id!)"
                        class="q-ml-xs"
                      />
                      <q-chip
                        v-else
                        dense
                        color="accent"
                        text-color="white"
                        :label="$t('mergeContent.selectedDescription')"
                        class="q-ml-xs"
                      />
                    </div>
                    <div class="text-body2">
                      {{ item.shortDescription }}
                    </div>
                  </div>

                  <!-- ID for debugging -->
                  <div class="text-caption text-grey-5">
                    ID: {{ item.id }}
                  </div>
                </q-card-section>
              </q-card>
            </div>
          </div>

          <!-- Action buttons -->
          <q-card-actions align="right" class="q-mt-lg">
            <q-btn
              flat
              color="grey"
              :label="$t('mergeContent.skip')"
              @click="skipMerge"
              :disable="merging"
            />
            <q-btn
              color="primary"
              :label="$t('mergeContent.merge')"
              @click="performMerge"
              :loading="merging"
              :disable="selectedItemIds.length < 2 || !currentDuplicate.items"
            />
          </q-card-actions>
        </q-card-section>
      </q-card>
    </div>

    <!-- Progress indicator -->
    <div v-if="duplicates.length > 0" class="text-center q-mt-lg">
      <div class="text-body2 text-grey-6">
        {{ $t('mergeContent.progress', {
          current: processedCount + 1,
          total: duplicates.length
        }) }}
      </div>
      <q-linear-progress
        :value="processedCount / (duplicates.length)"
        color="primary"
        class="q-mt-sm"
        style="height: 4px"
      />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import {
  Client,
  type PossibleDuplicateResponse,
  MergeContentRequest
} from 'src/api/client';

const { t } = useI18n();
const $q = useQuasar();

const client = new Client();

// State
const loading = ref(false);
const merging = ref(false);
const duplicates = ref<PossibleDuplicateResponse[]>([]);
const currentIndex = ref(0);
const processedCount = ref(0);
const selectedItemIds = ref<string[]>([]);
const selectedDescriptionId = ref<string | null>(null);

// Computed
const currentDuplicate = computed(() => {
  if (duplicates.value.length === 0 || currentIndex.value >= duplicates.value.length) {
    return null;
  }
  return duplicates.value[currentIndex.value];
});

// Methods
const fetchDuplicates = async () => {
  loading.value = true;
  try {
    const result = await client.getPossibleDuplicates();
    duplicates.value = result || [];
    currentIndex.value = 0;
    processedCount.value = 0;
    resetSelections();
  } catch (error) {
    console.error('Error fetching duplicates:', error);
    $q.notify({
      type: 'negative',
      message: t('mergeContent.errors.fetchFailed'),
    });
    duplicates.value = [];
  } finally {
    loading.value = false;
  }
};

const toggleItemSelection = (itemId: string) => {
  const index = selectedItemIds.value.indexOf(itemId);
  if (index > -1) {
    // Remove item from selection
    selectedItemIds.value.splice(index, 1);
  } else {
    // Add item to selection
    selectedItemIds.value.push(itemId);
  }

  // If no description is selected yet and we just selected the first item, use its description by default
  if (!selectedDescriptionId.value && selectedItemIds.value.length === 1) {
    const selectedItem = currentDuplicate.value?.items?.find(item => item.id === itemId);
    if (selectedItem?.shortDescription) {
      selectedDescriptionId.value = itemId;
    }
  }

  // If the description item is deselected, clear the description selection
  if (selectedDescriptionId.value === itemId && !selectedItemIds.value.includes(itemId)) {
    selectedDescriptionId.value = null;
  }
};

const selectDescription = (itemId: string) => {
  selectedDescriptionId.value = itemId;
};

const resetSelections = () => {
  selectedItemIds.value = [];
  selectedDescriptionId.value = null;
};

const performMerge = async () => {
  if (!currentDuplicate.value || selectedItemIds.value.length < 2 || !currentDuplicate.value.items) {
    return;
  }

  merging.value = true;
  try {
    const descriptionId = selectedDescriptionId.value || selectedItemIds.value[0];
    const mergeRequest = new MergeContentRequest({
      contentIds: selectedItemIds.value,
      ...(descriptionId && { descriptionContentId: descriptionId }),
    });

    await client.mergeContent(mergeRequest);

    $q.notify({
      type: 'positive',
      message: t('mergeContent.mergeSuccess'),
    });

    moveToNext();
  } catch (error) {
    console.error('Error merging content:', error);
    $q.notify({
      type: 'negative',
      message: t('mergeContent.errors.mergeFailed'),
    });
  } finally {
    merging.value = false;
  }
};

const skipMerge = () => {
  moveToNext();
};

const moveToNext = () => {
  processedCount.value++;
  if (currentIndex.value < duplicates.value.length - 1) {
    currentIndex.value++;
    resetSelections();
  } else {
    // No more duplicates, fetch fresh data
    void fetchDuplicates();
  }
};

// Lifecycle
onMounted(() => {
  void fetchDuplicates();
});
</script>

<style scoped>
.transition-all {
  transition: all 0.3s ease;
}

.cursor-pointer {
  cursor: pointer;
}

.cursor-pointer:hover {
  transform: scale(1.02);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
</style>
