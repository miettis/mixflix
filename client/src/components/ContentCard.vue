<template>
  <q-card class="content-card cursor-pointer" @click="$emit('view-content', content)">
    <!-- Content Image -->
    <div class="content-image-container">
      <q-img
        v-if="content.imageUrl"
        :src="content.imageUrl"
        :alt="content.title"
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

      <!-- Rating Badge -->
      <div v-if="showRatingBadge" class="rating-badge">
        <q-btn-dropdown flat dense no-caps class="rating-dropdown-btn" dropdown-icon="" @click.stop>
          <template v-slot:label>
            <span v-if="content.userRating" class="rating-emoji">{{
              getRatingEmoji(content.userRating)
            }}</span>
            <span v-else class="rating-text">{{ $t('content.rate') }}</span>
          </template>

          <q-list dense>
            <q-item
              v-for="rating in ratingOptions"
              :key="rating.value"
              clickable
              @click="$emit('update-rating', content, rating.value)"
              :class="{
                'bg-primary text-white': content.userRating === rating.value,
              }"
              v-close-popup
            >
              <q-item-section class="text-center">
                <span style="font-size: 1.8em">{{ rating.emoji }}</span>
              </q-item-section>
            </q-item>
          </q-list>
        </q-btn-dropdown>
      </div>
    </div>

    <!-- Content Info -->
    <q-card-section class="q-pa-sm">
      <div class="text-subtitle2 ellipsis-2-lines">{{ content.title }}</div>
      <div class="row items-center justify-between q-mt-xs">
        <div v-if="content.releaseYear" class="text-caption text-grey-6">
          {{ content.releaseYear }}
        </div>
        <div class="row q-gutter-xs items-center">
          <div v-if="content.imdbScore" class="rating-score">
            <q-img src="~assets/imdb.svg" class="imdb-logo" />
            <span class="score-text text-grey-6">{{ content.imdbScore }}</span>
          </div>
          <div v-if="content.tmdbScore" class="rating-score">
            <q-img src="~assets/tmdb.svg" class="tmdb-logo" />
            <span class="score-text text-grey-6">{{ Number(content.tmdbScore).toFixed(1) }}</span>
          </div>
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
import type { UserContentResponse } from 'src/api/client';

interface Props {
  content: UserContentResponse;
  showRatingBadge?: boolean;
  ratingOptions: Array<{ value: number; emoji: string }>;
}

const props = defineProps<Props>();

defineEmits<{
  'view-content': [content: UserContentResponse];
  'update-rating': [content: UserContentResponse, rating: number];
}>();

const getRatingEmoji = (rating: number) => {
  const ratingOption = props.ratingOptions.find((r) => r.value === rating);
  return ratingOption?.emoji || '⭐';
};
</script>

<style scoped>
.content-card {
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease;
  height: 100%;
}

.content-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

.content-image-container {
  position: relative;
  width: 100%;
  aspect-ratio: 3/4;
  overflow: hidden;
}

.content-image {
  height: 100%;
  object-fit: cover;
}

.content-image-placeholder {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f5f5f5;
}

.rating-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 2px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  font-size: 0.8rem;
}

.rating-dropdown-btn {
  color: white !important;
  min-height: unset !important;
  padding: 2px 6px !important;
}

.rating-dropdown-btn .q-btn__content {
  min-width: unset !important;
}

.rating-dropdown-btn .q-btn-dropdown__arrow {
  display: none !important;
}

.rating-dropdown-btn :deep(.q-btn-dropdown__arrow) {
  display: none !important;
}

.rating-dropdown-btn :deep(.q-icon) {
  display: none !important;
}

.rating-emoji {
  font-size: 1.5em;
}

.rating-text {
  font-size: 0.75em;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.ellipsis-2-lines {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.2em;
  max-height: 2.4em;
}

.rating-score {
  display: flex;
  align-items: center;
  gap: 4px;
}

.score-text {
  font-size: 0.75rem;
  font-weight: 500;
}

.imdb-logo {
  width: 24px;
  height: 12px;
}

.tmdb-logo {
  width: 24px;
  height: 10px;
}
</style>
