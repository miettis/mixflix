<template>
  <q-card class="group-suggestion-card cursor-pointer" @click="viewContent">
    <!-- Content Image -->
    <div class="content-image-container">
      <q-img
        v-if="suggestion.imageUrl"
        :src="suggestion.imageUrl"
        :alt="suggestion.title"
        class="content-image"
        loading="lazy"
      >
        <template v-slot:error>
          <div class="absolute-full flex flex-center bg-grey-3">
            <q-icon name="broken_image" size="50px" color="grey-6" />
          </div>
        </template>
      </q-img>
      <div v-else class="content-image-placeholder">
        <q-icon name="movie" size="50px" color="grey-6" />
      </div>

      <!-- Group Score Badge -->
      <div v-if="suggestion.averageScore" class="group-score-badge">
        <q-icon name="group" size="xs" class="q-mr-xs" />
        <span class="score-text">{{ suggestion.averageScore.toFixed(1) }}</span>
      </div>

      <!-- Weighted Score Chip -->
      <div v-if="suggestion.weightedScore" class="weighted-score-chip">
        <q-chip
          color="dark"
          text-color="white"
          size="md"
          dense
        >
          {{ suggestion.weightedScore.toFixed(1) }}
        </q-chip>
      </div>
    </div>

    <!-- Content Details -->
    <q-card-section class="content-details">
      <!-- Title -->
      <div class="text-h6 content-title">{{ suggestion.title }}</div>

      <!-- Release Year and Runtime -->
      <div v-if="suggestion.releaseYear" class="text-caption text-grey-6 q-mb-sm">
        {{ suggestion.releaseYear }}
        <span v-if="suggestion.runtime"> • {{ suggestion.runtime }} min</span>
        <span v-if="suggestion.type === 1"> • {{ $t('groups.contentTypes.movie') }}</span>
        <span v-if="suggestion.type === 2"> • {{ $t('groups.contentTypes.series') }}</span>
      </div>

      <!-- Group Statistics -->
      <div v-if="suggestion.ratingCount || suggestion.totalScore" class="group-stats q-mb-sm">
        <div class="text-caption text-primary">
          <q-icon name="people" size="xs" class="q-mr-xs" />
          <span v-if="suggestion.ratingCount">
            {{ $t('groups.suggestions.ratedBy', { count: suggestion.ratingCount }) }}
          </span>
          <span v-if="suggestion.totalScore && suggestion.ratingCount">
            • {{ $t('groups.suggestions.totalScore') }}: {{ suggestion.totalScore }}
          </span>
        </div>
      </div>

      <!-- External Ratings -->
      <div class="external-ratings row items-center q-gutter-sm q-mb-sm">
        <div v-if="suggestion.imdbScore" class="rating-score">
          <q-img src="~assets/imdb.svg" class="imdb-logo" />
          <span class="rating-value">{{ suggestion.imdbScore.toFixed(1) }}</span>
        </div>

        <div v-if="suggestion.tmdbScore" class="rating-score">
          <q-img src="~assets/tmdb.svg" class="tmdb-logo" />
          <span class="rating-value">{{ suggestion.tmdbScore.toFixed(1) }}</span>
        </div>
      </div>

      <!-- Categories -->
      <div v-if="suggestion.categories && suggestion.categories.length > 0" class="categories q-mb-sm">
        <q-chip
          v-for="(category, index) in suggestion.categories.slice(0, 3)"
          :key="category.id || category.name || `category-${index}`"
          size="sm"
          color="secondary"
          text-color="white"
          class="q-mr-xs q-mb-xs"
        >
          {{ category.name }}
        </q-chip>
        <span v-if="suggestion.categories.length > 3" class="text-caption text-grey-6">
          +{{ suggestion.categories.length - 3 }} {{ $t('groups.suggestions.more') }}
        </span>
      </div>

      <!-- Availability -->
      <div
        v-if="suggestion.availabilities && suggestion.availabilities.length > 0"
        class="availability q-mt-sm"
      >
        <div class="availability-services">
          <q-chip
            v-for="(availability, index) in suggestion.availabilities.slice(0, 4)"
            :key="availability.id || availability.serviceName || `availability-${index}`"
            color="primary"
            text-color="white"
            icon="play_circle"
            class="q-mr-xs"
          >
            {{ availability.serviceName }}
          </q-chip>
          <span v-if="suggestion.availabilities.length > 4" class="text-caption text-grey-6">
            +{{ suggestion.availabilities.length - 4 }}
          </span>
        </div>
      </div>
    </q-card-section>

    <!-- Content Info Dialog -->
    <q-dialog v-model="showContentDialog" @hide="closeDialog">
      <q-card style="min-width: 400px; max-width: 600px">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">{{ suggestion.title || $t('content.dialog.title') }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <div class="row q-gutter-md">
            <div class="col-5">
              <!-- Content Image -->
              <div class="row">
                <div class="col">
                  <div class="content-dialog-image">
                    <q-img
                      v-if="suggestion.imageUrl"
                      :src="suggestion.imageUrl"
                      :alt="suggestion.title"
                      class="rounded-borders"
                    >
                      <template v-slot:error>
                        <div class="absolute-full flex flex-center bg-grey-3">
                          <q-icon name="broken_image" size="50px" color="grey-6" />
                        </div>
                      </template>
                    </q-img>
                    <div v-else class="content-dialog-placeholder">
                      <q-icon name="movie" size="50px" color="grey-6" />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Release Year and Runtime -->
              <div class="row q-mt-sm text-center">
                <div v-if="suggestion.releaseYear">
                  <q-chip color="grey-4" text-color="black" icon="event">
                    {{ suggestion.releaseYear }}
                  </q-chip>
                </div>
                <div v-if="suggestion.runtime">
                  <q-chip color="grey-4" text-color="black" icon="schedule">
                    {{ suggestion.runtime }} {{ $t('content.dialog.minutes') }}
                  </q-chip>
                </div>
              </div>

              <!-- Group Score -->
              <div v-if="suggestion.averageScore" class="row q-mt-sm text-center">
                <q-chip color="teal" text-color="white" icon="group">
                  {{ $t('groups.suggestions.groupScore') }}:
                  {{ suggestion.averageScore.toFixed(1) }}
                </q-chip>
              </div>

              <!-- External Ratings -->
              <div class="row q-mt-sm text-center">
                <div v-if="suggestion.imdbScore" class="rating-score dialog-rating">
                  <q-img src="~assets//imdb.svg" class="imdb-logo" />
                  <span class="score-text text-grey-6">{{ suggestion.imdbScore.toFixed(1) }}</span>
                </div>
                <div v-if="suggestion.tmdbScore" class="rating-score dialog-rating">
                  <q-img src="~assets/tmdb.svg" class="tmdb-logo" />
                  <span class="score-text text-grey-6">{{ suggestion.tmdbScore.toFixed(1) }}</span>
                </div>
              </div>

              <!-- Categories -->
              <div
                v-if="suggestion.categories && suggestion.categories.length > 0"
                class="row q-mt-sm"
              >
                <q-chip
                  v-for="category in suggestion.categories"
                  :key="`category-${category.id}`"
                  color="secondary"
                  text-color="white"
                  class="q-mr-xs q-mb-xs"
                >
                  {{ category.name }}
                </q-chip>
              </div>

              <!-- Available Services -->
              <div
                v-if="suggestion.availabilities && suggestion.availabilities.length > 0"
                class="q-mt-sm"
              >
                <div class="text-subtitle2 q-mb-xs">{{ $t('content.dialog.availableOn') }}</div>
                <div class="row q-gutter-xs">
                  <q-chip
                    v-for="availability in suggestion.availabilities"
                    :key="availability.id || availability.serviceName"
                    color="primary"
                    text-color="white"
                    icon="play_circle"
                    class="q-mr-xs q-mb-xs"
                  >
                    {{ availability.serviceName }}
                  </q-chip>
                </div>
              </div>
            </div>

            <!-- Content Details -->
            <div class="col">
              <div v-if="suggestion.shortDescription" class="q-mb-sm">
                <div class="text-body2 text-grey-6">{{ suggestion.shortDescription }}</div>
              </div>

              <!-- Group Statistics -->
              <div
                v-if="suggestion.ratingCount || suggestion.totalScore"
                class="group-stats-dialog q-mb-sm"
              >
                <div class="text-subtitle2 q-mb-xs">{{ $t('groups.suggestions.groupStats') }}</div>
                <div class="text-caption text-primary">
                  <q-icon name="people" size="xs" class="q-mr-xs" />
                  <span v-if="suggestion.ratingCount">
                    {{ $t('groups.suggestions.ratedBy', { count: suggestion.ratingCount }) }}
                  </span>
                  <span v-if="suggestion.totalScore && suggestion.ratingCount">
                    • {{ $t('groups.suggestions.totalScore') }}: {{ suggestion.totalScore }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="$t('common.close')" color="primary" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { GroupSuggestion } from 'src/api/client';

interface Props {
  suggestion: GroupSuggestion;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  'view-content': [suggestion: GroupSuggestion];
}>();

const showContentDialog = ref(false);

const viewContent = () => {
  showContentDialog.value = true;
  emit('view-content', props.suggestion);
};

const closeDialog = () => {
  showContentDialog.value = false;
};
</script>

<style scoped>
.group-suggestion-card {
  height: 100%;
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease;
  overflow: hidden;
}

.group-suggestion-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.content-image-container {
  position: relative;
  width: 100%;
  aspect-ratio: 2/3; /* Standard movie poster ratio */
  overflow: hidden;
}

.content-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.content-image-placeholder {
  width: 100%;
  height: 100%;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
}

.group-score-badge {
  position: absolute;
  top: 40px;
  right: 8px;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  font-size: 0.75rem;
  font-weight: 500;
}

.weighted-score-chip {
  position: absolute;
  top: 8px;
  right: 4px;
  z-index: 2;
}

.score-text {
  font-weight: 600;
}

.content-details {
  padding: 12px;
  flex-grow: 1;
}

.content-title {
  font-size: 1.1rem;
  font-weight: 600;
  line-height: 1.3;
  margin-bottom: 6px;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  overflow: hidden;
}

.content-description {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  overflow: hidden;
  line-height: 1.4;
  color: #666;
}

.group-stats {
  background: rgba(0, 216, 195, 0.1);
  padding: 4px 6px;
  border-radius: 4px;
  border-left: 2px solid #00d8c3;
}

.external-ratings {
  min-height: 20px;
}

.rating-score {
  display: flex;
  align-items: center;
  gap: 4px;
}

.imdb-logo {
  width: 24px;
  height: 12px;
}

.tmdb-logo {
  width: 24px;
  height: 10px;
}

.rating-value {
  font-size: 0.75rem;
  font-weight: 500;
  color: #666;
}

.categories {
  margin-top: 4px;
}

.availability {
  border-top: 1px solid #eee;
  padding-top: 8px;
}

.availability-services {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  align-items: center;
}

.content-dialog-image {
  width: 100%;
  aspect-ratio: 2/3;
  overflow: hidden;
}

.content-dialog-placeholder {
  width: 100%;
  height: 100%;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
}

.dialog-rating {
  margin: 0 8px;
}

.group-stats-dialog {
  background: rgba(0, 216, 195, 0.1);
  padding: 12px;
  border-radius: 8px;
  border-left: 3px solid #00d8c3;
}

/* Dark theme adjustments */
.body--dark .content-image-placeholder {
  background: #333;
}

.body--dark .content-description {
  color: #bbb;
}

.body--dark .rating-value {
  color: #aaa;
}

.body--dark .availability {
  border-top-color: #444;
}
</style>
