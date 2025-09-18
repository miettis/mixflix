<template>
  <q-page class="q-pa-md">
    <div class="row justify-center">
      <div class="col-12">
        <!-- Tab Navigation -->
        <q-tabs
          v-model="activeTab"
          dense
          class="text-grey q-mb-lg"
          active-color="primary"
          indicator-color="primary"
          align="justify"
        >
          <q-tab name="my-rated" :label="$t('content.tabs.myRated')" />
          <q-tab name="search" :label="$t('content.tabs.search')" />
        </q-tabs>

        <!-- Tab Panels -->
        <q-tab-panels v-model="activeTab" animated>
          <!-- My Rated Content Tab -->
          <q-tab-panel name="my-rated" class="q-pa-none">
            <!-- Loading state -->
            <div v-if="loadingMyRated" class="row justify-center q-pa-lg">
              <q-spinner-dots size="50px" color="primary" />
              <div class="text-body2 q-mt-md">{{ $t('content.myRated.loading') }}</div>
            </div>

            <!-- My Rated Results Grid -->
            <div v-else-if="myRatedContent.length > 0" class="q-mb-lg">
              <div class="row items-center justify-between q-mb-md">
                <div class="text-h6">
                  {{ $t('content.myRated.results', { count: myRatedTotalCount }) }}
                </div>
                <div class="row items-center">
                  <div class="text-subtitle2 q-mr-sm">{{ $t('content.sorting.label') }}:</div>
                  <q-select
                    v-model="myRatedSorting"
                    :options="sortingOptions"
                    option-label="label"
                    option-value="value"
                    emit-value
                    map-options
                    dense
                    outlined
                    class="sorting-select"
                    @update:model-value="loadMyRatedContent"
                  >
                    <template v-slot:selected-item="scope">
                      <q-icon :name="scope.opt.icon" class="q-mr-xs" />
                      {{ scope.opt.label }}
                    </template>
                    <template v-slot:option="scope">
                      <q-item v-bind="scope.itemProps">
                        <q-item-section avatar>
                          <q-icon :name="scope.opt.icon" />
                        </q-item-section>
                        <q-item-section>
                          <q-item-label>{{ scope.opt.label }}</q-item-label>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select>
                </div>
              </div>
              <div class="row">
                <div
                  v-for="content in myRatedContent"
                  :key="content.id || 'unknown'"
                  class="col-6 col-md-3 col-lg-2 q-pa-sm"
                >
                  <ContentCard
                    :content="content"
                    :show-rating-badge="!!content.userRating"
                    :rating-options="ratingOptions"
                    @view-content="viewContent"
                    @update-rating="updateContentRating"
                  />
                </div>
              </div>

              <!-- Load More Button for My Rated -->
              <div v-if="hasMoreMyRated" class="row justify-center q-mt-md">
                <q-btn
                  color="primary"
                  :label="$t('content.loadMore')"
                  :loading="loadingMoreMyRated"
                  @click="loadMyRatedContent(true)"
                  outline
                />
              </div>
            </div>

            <!-- No My Rated Content -->
            <div v-else class="text-center q-pa-lg">
              <q-icon name="star_border" size="4rem" color="grey-5" />
              <div class="text-h6 q-mt-md text-grey-6">{{ $t('content.myRated.noResults') }}</div>
              <div class="text-body2 text-grey-5">
                {{ $t('content.myRated.noResultsSubtitle') }}
              </div>
            </div>
          </q-tab-panel>

          <!-- Search Tab -->
          <q-tab-panel name="search" class="q-pa-none">
            <!-- Title Search -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">
                {{ $t('content.search.titleLabel') }}
              </div>
              <q-input
                v-model="titleFilter"
                :placeholder="$t('content.search.titlePlaceholder')"
                outlined
                dense
                clearable
                class="full-width"
                @keyup.enter="searchContent()"
              />
            </div>

            <!-- Services -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">
                {{ $t('groups.createDialog.servicesLabel') }}
              </div>
              <div class="row q-gutter-sm">
                <q-chip
                  v-for="(service, index) in availableServices"
                  :key="service.id || `service-${index}`"
                  :selected="selectedServices.includes(service.id || '')"
                  clickable
                  color="primary"
                  text-color="white"
                  @click="toggleService(service.id || '')"
                >
                  {{ service.name }}
                </q-chip>
              </div>
            </div>

            <!-- Content Types -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">
                {{ $t('groups.createDialog.contentTypesLabel') }}
              </div>
              <div class="row q-gutter-sm">
                <q-chip
                  v-for="contentType in contentTypes"
                  :key="contentType.value"
                  :selected="selectedContentTypes.includes(contentType.value)"
                  clickable
                  color="accent"
                  text-color="white"
                  @click="toggleContentType(contentType.value)"
                >
                  <q-icon :name="contentType.icon" class="q-mr-xs" />
                  {{ contentType.label }}
                </q-chip>
              </div>
            </div>

            <!-- Categories -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">
                {{ $t('groups.createDialog.categoriesLabel') }}
              </div>
              <div class="row q-gutter-sm">
                <q-chip
                  v-for="(category, index) in availableCategories"
                  :key="category.id || `category-${index}`"
                  :selected="selectedCategories.includes(category.id || '')"
                  clickable
                  color="secondary"
                  text-color="white"
                  @click="toggleCategory(category.id || '')"
                >
                  {{ category.name }}
                </q-chip>
              </div>
            </div>

            <!-- Review Status -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">
                {{ $t('content.search.reviewStatusLabel') }}
              </div>
              <div class="row q-gutter-sm">
                <q-chip
                  :selected="reviewedFilter === true"
                  clickable
                  color="positive"
                  text-color="white"
                  @click="toggleReviewedFilter(true)"
                >
                  {{ $t('content.search.reviewedLabel') }}
                </q-chip>
                <q-chip
                  :selected="reviewedFilter === false"
                  clickable
                  color="warning"
                  text-color="white"
                  @click="toggleReviewedFilter(false)"
                >
                  {{ $t('content.search.notReviewedLabel') }}
                </q-chip>
              </div>
            </div>

            <!-- Ratings Filter -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">{{ $t('content.search.ratingsLabel') }}</div>
              <div class="row q-gutter-sm">
                <q-chip
                  v-for="rating in ratingOptions"
                  :key="rating.value"
                  :selected="selectedRatings.includes(rating.value)"
                  clickable
                  color="grey-6"
                  text-color="white"
                  @click="toggleRating(rating.value)"
                >
                  <span style="font-size: 1.2em">{{ rating.emoji }}</span>
                  <span class="q-ml-xs">{{ rating.value }}</span>
                </q-chip>
              </div>
            </div>

            <!-- Sorting -->
            <div class="q-mb-md">
              <div class="text-subtitle2 q-mb-sm">{{ $t('content.sorting.label') }}</div>
              <q-select
                v-model="selectedSorting"
                :options="sortingOptions"
                option-label="label"
                option-value="value"
                emit-value
                map-options
                dense
                outlined
                class="sorting-select"
              >
                <template v-slot:selected-item="scope">
                  <q-icon :name="scope.opt.icon" class="q-mr-xs" />
                  {{ scope.opt.label }}
                </template>
                <template v-slot:option="scope">
                  <q-item v-bind="scope.itemProps">
                    <q-item-section avatar>
                      <q-icon :name="scope.opt.icon" />
                    </q-item-section>
                    <q-item-section>
                      <q-item-label>{{ scope.opt.label }}</q-item-label>
                    </q-item-section>
                  </q-item>
                </template>
              </q-select>
            </div>

            <!-- Search Button -->
            <div class="row justify-center">
              <q-btn
                color="primary"
                :label="$t('content.search.searchButton')"
                :loading="searching"
                @click="() => searchContent()"
                icon="search"
              />
            </div>

            <!-- Loading state -->
            <div v-if="searching" class="row justify-center q-pa-lg">
              <q-spinner-dots size="50px" color="primary" />
            </div>

            <!-- Results Grid -->
            <div v-else-if="searchResults.length > 0" class="q-mb-lg">
              <div class="text-h6 q-mb-md">
                {{ $t('content.search.results', { count: searchTotalCount }) }}
              </div>
              <div class="row">
                <div
                  v-for="content in searchResults"
                  :key="content.id || 'unknown'"
                  class="col-6 col-md-3 col-lg-2 q-pa-sm"
                >
                  <ContentCard
                    :content="content"
                    :show-rating-badge="true"
                    :rating-options="ratingOptions"
                    @view-content="viewContent"
                    @update-rating="updateContentRating"
                  />
                </div>
              </div>

              <!-- Load More Button for Search Results -->
              <div v-if="hasMoreSearch" class="row justify-center q-mt-md">
                <q-btn
                  color="primary"
                  :label="$t('content.loadMore')"
                  :loading="loadingMoreSearch"
                  @click="searchContent(true)"
                  outline
                />
              </div>
            </div>

            <!-- No Results -->
            <div v-else-if="hasSearched && !searching" class="text-center q-pa-lg">
              <q-icon name="search_off" size="4rem" color="grey-5" />
              <div class="text-h6 q-mt-md text-grey-6">{{ $t('content.search.noResults') }}</div>
              <div class="text-body2 text-grey-5">
                {{ $t('content.search.tryDifferentCriteria') }}
              </div>
            </div>

            <!-- Initial State -->
            <div v-else-if="!hasSearched" class="text-center q-pa-lg">
              <q-icon name="search" size="4rem" color="grey-5" />
              <div class="text-h6 q-mt-md text-grey-6">{{ $t('content.search.welcome') }}</div>
              <div class="text-body2 text-grey-5">
                {{ $t('content.search.selectCriteria') }}
              </div>
            </div>
          </q-tab-panel>
        </q-tab-panels>

        <!-- Content Info Dialog -->
        <q-dialog v-model="showContentDialog" @hide="selectedContent = null">
          <q-card style="min-width: 400px; max-width: 600px">
            <q-card-section class="row items-center q-pb-none">
              <div class="text-h6">{{ selectedContent?.title || $t('content.dialog.title') }}</div>
              <q-space />
              <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="selectedContent">
              <div class="row q-gutter-md">
                <div class="col-5">
                  <!-- Content Image -->
                  <div class="row">
                    <div class="col">
                      <div class="content-dialog-image">
                        <q-img
                          v-if="selectedContent.imageUrl"
                          :src="selectedContent.imageUrl"
                          :alt="selectedContent.title"
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
                    <div v-if="selectedContent.releaseYear">
                      <q-chip color="grey-4" text-color="black" icon="event">
                        {{ selectedContent.releaseYear }}
                      </q-chip>
                    </div>
                    <div v-if="selectedContent.runtime">
                      <q-chip color="grey-4" text-color="black" icon="schedule">
                        {{ selectedContent.runtime }} {{ $t('content.dialog.minutes') }}
                      </q-chip>
                    </div>
                  </div>

                  <!-- Ratings -->
                  <div class="row q-mt-sm text-center">
                    <div v-if="selectedContent.imdbScore" class="rating-score dialog-rating">
                      <q-img src="~assets/imdb.svg" class="imdb-logo" />
                      <span class="score-text text-grey-6">{{ selectedContent.imdbScore }}</span>
                    </div>
                    <div v-if="selectedContent.tmdbScore" class="rating-score dialog-rating">
                      <q-img src="~assets/tmdb.svg" class="tmdb-logo" />
                      <span class="score-text text-grey-6">{{
                        Number(selectedContent.tmdbScore).toFixed(1)
                      }}</span>
                    </div>
                  </div>

                  <!-- Categories -->
                  <div class="row q-mt-sm">
                    <q-chip
                      v-for="category in selectedContent.categories"
                      :key="`category-${category.id}`"
                      color="secondary"
                      text-color="white"
                    >
                      {{ category.name }}
                    </q-chip>
                  </div>

                  <!-- Languages -->
                  <div v-if="selectedContent.languages && selectedContent.languages.length > 0" class="q-mt-sm">
                    <div class="text-subtitle2 q-mb-xs">{{ $t('content.dialog.audio') }}</div>
                    <div class="row q-gutter-xs">
                      <q-chip
                        v-for="language in selectedContent.languages"
                        :key="language"
                        color="grey-4"
                        text-color="black"
                        size="sm"
                      >
                        {{ getTranslatedLanguageName(language) }}
                      </q-chip>
                    </div>
                  </div>

                  <!-- Available Services -->
                  <div
                    v-if="
                      selectedContent.availabilities && selectedContent.availabilities.length > 0
                    "
                    class="q-mt-sm"
                  >
                    <div class="text-subtitle2 q-mb-xs">{{ $t('content.dialog.availableOn') }}</div>
                    <div class="row q-gutter-xs">
                      <q-chip
                        v-for="availability in selectedContent.availabilities"
                        :key="availability.id || availability.name"
                        color="primary"
                        text-color="white"
                        icon="play_circle"
                      >
                        {{ availability.serviceName }}
                      </q-chip>
                    </div>
                  </div>
                </div>

                <!-- Content Details -->
                <div class="col">
                  <div v-if="selectedContent.shortDescription" class="q-mb-sm">
                    <div class="text-body2 text-grey-6">{{ selectedContent.shortDescription }}</div>
                  </div>

                  <!-- Cast -->
                  <div v-if="selectedContent.cast && selectedContent.cast.length > 0" class="q-mt-sm">
                    <div class="text-subtitle2 q-mb-xs">{{ $t('content.dialog.cast') }}</div>
                    <div class="text-body2 text-grey-7">
                      {{ selectedContent.cast.join(', ') }}
                    </div>
                  </div>
                </div>
              </div>

              <!-- Rating -->
              <div class="row justify-center">
                <div class="col-12">
                  <div class="q-mb-sm text-center">
                    <div class="rating-section q-pa-md rounded-borders">
                      <div class="row justify-center q-gutter-sm">
                        <q-btn
                          v-for="rating in ratingOptions"
                          :key="rating.value"
                          :color="selectedContent.userRating === rating.value ? 'primary' : 'white'"
                          :text-color="
                            selectedContent.userRating === rating.value ? 'white' : 'grey-7'
                          "
                          :outline="selectedContent.userRating !== rating.value"
                          size="lg"
                          round
                          :loading="updatingRating"
                          @click="updateRating(rating.value)"
                          class="rating-btn-enhanced"
                          :class="{
                            'rating-btn-selected': selectedContent.userRating === rating.value,
                          }"
                        >
                          <span style="font-size: 1.5em">{{ rating.emoji }}</span>
                          <q-tooltip class="text-body2">
                            {{ $t('content.dialog.rateWith') }} {{ rating.value }}/5
                          </q-tooltip>
                        </q-btn>
                      </div>
                      <div class="text-caption text-grey-6 q-mt-sm">
                        {{ $t('content.dialog.clickToRate') }}
                      </div>
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

        <!-- Scroll to Top Button -->
        <q-page-sticky position="bottom-right" :offset="[18, 18]">
          <q-btn
            v-show="showScrollToTop"
            fab
            icon="keyboard_arrow_up"
            color="primary"
            @click="scrollToTop"
            class="scroll-to-top-btn"
          >
            <q-tooltip>{{ $t('common.scrollToTop') }}</q-tooltip>
          </q-btn>
        </q-page-sticky>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import {
  Client,
  type ServiceResponse,
  type CategoryResponse,
  type UserContentResponse,
  SearchContentRequest,
  ReviewRequest,
  ContentSorting,
} from 'src/api/client';
import ContentCard from 'src/components/ContentCard.vue';

const { t } = useI18n();
const $q = useQuasar();

const client = new Client();

// State
const activeTab = ref('my-rated');
const availableServices = ref<ServiceResponse[]>([]);
const availableCategories = ref<CategoryResponse[]>([]);
const selectedServices = ref<string[]>([]);
const selectedCategories = ref<string[]>([]);
const selectedContentTypes = ref<number[]>([]);
const selectedRatings = ref<number[]>([]);
const titleFilter = ref<string>('');
const reviewedFilter = ref<boolean | null>(null);
const selectedSorting = ref<ContentSorting>(ContentSorting.None);
const myRatedSorting = ref<ContentSorting>(ContentSorting.None);
const searchResults = ref<UserContentResponse[]>([]);
const myRatedContent = ref<UserContentResponse[]>([]);
const searching = ref(false);
const loadingMyRated = ref(false);
const loadingMoreMyRated = ref(false);
const loadingMoreSearch = ref(false);
const hasSearched = ref(false);
const showContentDialog = ref(false);
const selectedContent = ref<UserContentResponse | null>(null);
const updatingRating = ref(false);
const showScrollToTop = ref(false);

// Pagination state
const PAGE_SIZE = 48;
const myRatedOffset = ref(0);
const searchOffset = ref(0);
const hasMoreMyRated = ref(true);
const hasMoreSearch = ref(true);
const myRatedTotalCount = ref(0);
const searchTotalCount = ref(0);

// Content types options (1=movie, 2=series)
const contentTypes = computed(() => [
  { value: 1, label: t('groups.contentTypes.movie'), icon: 'movie' },
  { value: 2, label: t('groups.contentTypes.series'), icon: 'tv' },
]);

// Sorting options
const sortingOptions = computed(() => [
  { value: ContentSorting.None, label: t('content.sorting.none'), icon: 'sort' },
  {
    value: ContentSorting.Alphabetical,
    label: t('content.sorting.alphabetical'),
    icon: 'sort_by_alpha',
  },
  {
    value: ContentSorting.MostPopular,
    label: t('content.sorting.mostPopular'),
    icon: 'trending_up',
  },
  { value: ContentSorting.YearAscending, label: t('content.sorting.yearAscending'), icon: 'event' },
  {
    value: ContentSorting.YearDescending,
    label: t('content.sorting.yearDescending'),
    icon: 'event',
  },
  { value: ContentSorting.ImdbRating, label: t('content.sorting.imdbRating'), icon: 'star' },
  { value: ContentSorting.TmdbRating, label: t('content.sorting.tmdbRating'), icon: 'star' },
  {
    value: ContentSorting.MyRatingAscending,
    label: t('content.sorting.myRatingAscending'),
    icon: 'star_border',
  },
  {
    value: ContentSorting.MyRatingDescending,
    label: t('content.sorting.myRatingDescending'),
    icon: 'star',
  },
  {
    value: ContentSorting.MyRatingTimeAscending,
    label: t('content.sorting.myRatingTimeAscending'),
    icon: 'schedule',
  },
  {
    value: ContentSorting.MyRatingTimeDescending,
    label: t('content.sorting.myRatingTimeDescending'),
    icon: 'schedule',
  },
]);

// Rating options with emojis
const ratingOptions = [
  { value: 1, emoji: '😞' }, // Very sad/disappointed
  { value: 2, emoji: '😕' }, // Slightly disappointed
  { value: 3, emoji: '😐' }, // Neutral/okay
  { value: 4, emoji: '😊' }, // Happy/good
  { value: 5, emoji: '😍' }, // Love it/excellent
];

// Methods
const getTranslatedLanguageName = (language: string): string => {
  const normalizedLanguage = language.toLowerCase().trim();
  const languageKey = `content.languages.${normalizedLanguage}`;

  // Try to get translated name, fallback to original if not found
  const translatedName = t(languageKey);
  if (translatedName === languageKey) {
    // Translation not found, return the original language name
    return language;
  }

  return translatedName;
};

const fetchServices = async () => {
  try {
    availableServices.value = await client.getServices();
  } catch (error) {
    console.error('Error fetching services:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.servicesFetchFailed'),
    });
    availableServices.value = [];
  }
};

const fetchCategories = async () => {
  try {
    availableCategories.value = await client.getCategories();
  } catch (error) {
    console.error('Error fetching categories:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.categoriesFetchFailed'),
    });
    availableCategories.value = [];
  }
};

const loadMyRatedContent = async (loadMore = false) => {
  try {
    if (loadMore) {
      loadingMoreMyRated.value = true;
    } else {
      loadingMyRated.value = true;
      myRatedOffset.value = 0;
      hasMoreMyRated.value = true;
    }

    // Create request for reviewed content only
    const searchRequest = new SearchContentRequest();
    searchRequest.reviewed = true;
    searchRequest.sortBy = myRatedSorting.value;
    searchRequest.pageSize = PAGE_SIZE;
    searchRequest.offset = loadMore ? myRatedOffset.value : 0;

    const response = await client.searchContent(searchRequest);
    const results = response.items || [];

    if (loadMore) {
      myRatedContent.value.push(...results);
    } else {
      myRatedContent.value = results;
      myRatedTotalCount.value = response.totalCount || 0;
    }

    // Update pagination state
    if (results.length < PAGE_SIZE) {
      hasMoreMyRated.value = false;
    } else {
      myRatedOffset.value += PAGE_SIZE;
    }
  } catch (error) {
    console.error('Error loading my rated content:', error);
    $q.notify({
      type: 'negative',
      message: t('content.myRated.loadFailed'),
    });
    if (!loadMore) {
      myRatedContent.value = [];
    }
  } finally {
    loadingMyRated.value = false;
    loadingMoreMyRated.value = false;
  }
};

const toggleService = (serviceId: string) => {
  const index = selectedServices.value.indexOf(serviceId);
  if (index > -1) {
    selectedServices.value.splice(index, 1);
  } else {
    selectedServices.value.push(serviceId);
  }
};

const toggleCategory = (categoryId: string) => {
  const index = selectedCategories.value.indexOf(categoryId);
  if (index > -1) {
    selectedCategories.value.splice(index, 1);
  } else {
    selectedCategories.value.push(categoryId);
  }
};

const toggleContentType = (contentType: number) => {
  const index = selectedContentTypes.value.indexOf(contentType);
  if (index > -1) {
    selectedContentTypes.value.splice(index, 1);
  } else {
    selectedContentTypes.value.push(contentType);
  }
};

const toggleRating = (rating: number) => {
  const index = selectedRatings.value.indexOf(rating);
  if (index > -1) {
    selectedRatings.value.splice(index, 1);
  } else {
    selectedRatings.value.push(rating);
  }
};

const toggleReviewedFilter = (reviewed: boolean) => {
  if (reviewedFilter.value === reviewed) {
    reviewedFilter.value = null; // Deselect if already selected
  } else {
    reviewedFilter.value = reviewed;
  }
};

const searchContent = async (loadMore = false) => {
  if (loadMore) {
    loadingMoreSearch.value = true;
  } else {
    searching.value = true;
    hasSearched.value = true;
    searchOffset.value = 0;
    hasMoreSearch.value = true;
  }

  try {
    // Create the request object with proper type handling
    const searchRequest = new SearchContentRequest();

    // Only set properties if they have values
    if (titleFilter.value.trim()) {
      searchRequest.title = titleFilter.value.trim();
    }
    if (selectedServices.value.length > 0) {
      searchRequest.services = selectedServices.value;
    }
    if (selectedContentTypes.value.length > 0) {
      searchRequest.contentTypes = selectedContentTypes.value;
    }
    if (selectedCategories.value.length > 0) {
      searchRequest.categories = selectedCategories.value;
    }
    if (selectedRatings.value.length > 0) {
      searchRequest.ratings = selectedRatings.value;
    }
    if (reviewedFilter.value === true) {
      searchRequest.reviewed = true;
    } else if (reviewedFilter.value === false) {
      searchRequest.notReviewed = true;
    }
    if (selectedSorting.value !== ContentSorting.None) {
      searchRequest.sortBy = selectedSorting.value;
    }

    // Add pagination
    searchRequest.pageSize = PAGE_SIZE;
    searchRequest.offset = loadMore ? searchOffset.value : 0;

    const response = await client.searchContent(searchRequest);
    const results = response.items || [];

    if (loadMore) {
      searchResults.value.push(...results);
    } else {
      searchResults.value = results;
      searchTotalCount.value = response.totalCount || 0;
    }

    // Update pagination state
    if (results.length < PAGE_SIZE) {
      hasMoreSearch.value = false;
    } else {
      searchOffset.value += PAGE_SIZE;
    }
  } catch (error) {
    console.error('Error searching content:', error);
    $q.notify({
      type: 'negative',
      message: t('content.search.searchFailed'),
    });
    if (!loadMore) {
      searchResults.value = [];
    }
  } finally {
    searching.value = false;
    loadingMoreSearch.value = false;
  }
};

const viewContent = (content: UserContentResponse) => {
  selectedContent.value = content;
  showContentDialog.value = true;
};

const updateRating = async (newRating: number) => {
  if (!selectedContent.value?.id) {
    $q.notify({
      type: 'negative',
      message: t('content.dialog.updateRatingFailed'),
    });
    return;
  }

  try {
    updatingRating.value = true;

    // Create review request with the new rating
    const reviewRequest = new ReviewRequest();
    reviewRequest.rating = newRating;
    reviewRequest.type = selectedContent.value.contentType || 1; // Default to movie if not specified

    // Update the rating via API
    await client.reviewContent(selectedContent.value.id, reviewRequest);

    // Update the local content
    selectedContent.value.userRating = newRating;

    // Update in the lists as well
    const updateContentInList = (list: UserContentResponse[]) => {
      const index = list.findIndex((c) => c.id === selectedContent.value?.id);
      if (index !== -1 && list[index]) {
        list[index].userRating = newRating;
      }
    };

    updateContentInList(searchResults.value);
    updateContentInList(myRatedContent.value);

    $q.notify({
      type: 'positive',
      message: t('content.dialog.ratingUpdated'),
    });
  } catch (error) {
    console.error('Error updating rating:', error);
    $q.notify({
      type: 'negative',
      message: t('content.dialog.updateRatingFailed'),
    });
  } finally {
    updatingRating.value = false;
  }
};

const updateContentRating = async (content: UserContentResponse, newRating: number) => {
  if (!content.id) {
    $q.notify({
      type: 'negative',
      message: t('content.dialog.updateRatingFailed'),
    });
    return;
  }

  try {
    // Create review request with the new rating
    const reviewRequest = new ReviewRequest();
    reviewRequest.rating = newRating;
    reviewRequest.type = content.contentType || 1; // Default to movie if not specified

    // Update the rating via API
    await client.reviewContent(content.id, reviewRequest);

    // Update the local content
    content.userRating = newRating;

    // Update in both lists
    const updateContentInList = (list: UserContentResponse[]) => {
      const index = list.findIndex((c) => c.id === content.id);
      if (index !== -1 && list[index]) {
        list[index].userRating = newRating;
      }
    };

    updateContentInList(searchResults.value);
    updateContentInList(myRatedContent.value);

    $q.notify({
      type: 'positive',
      message: t('content.dialog.ratingUpdated'),
    });
  } catch (error) {
    console.error('Error updating rating:', error);
    $q.notify({
      type: 'negative',
      message: t('content.dialog.updateRatingFailed'),
    });
  }
};

const scrollToTop = () => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth',
  });
};

const handleScroll = () => {
  showScrollToTop.value = window.scrollY > 300;
};

onMounted(() => {
  void Promise.all([fetchServices(), fetchCategories()]);
  // Load my rated content by default since it's the first tab
  void loadMyRatedContent();

  // Add scroll event listener
  window.addEventListener('scroll', handleScroll);
});

onUnmounted(() => {
  // Clean up scroll event listener
  window.removeEventListener('scroll', handleScroll);
});

// Watch for tab changes to load data when needed
watch(activeTab, (newTab) => {
  if (newTab === 'my-rated' && myRatedContent.value.length === 0) {
    void loadMyRatedContent();
  }
});

// Watch for sorting changes to reset search and start from beginning
watch(selectedSorting, () => {
  if (hasSearched.value && searchResults.value.length > 0) {
    // Reset search state and search from the beginning
    searchOffset.value = 0;
    hasMoreSearch.value = true;
    void searchContent();
  }
});

// Watch for my rated sorting changes to reset and reload from beginning
watch(myRatedSorting, () => {
  if (myRatedContent.value.length > 0) {
    // Reset my rated state and load from the beginning
    myRatedOffset.value = 0;
    hasMoreMyRated.value = true;
    void loadMyRatedContent();
  }
});
</script>

<style scoped>
.content-dialog-placeholder {
  width: 150px;
  height: 200px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f5f5f5;
  border-radius: 4px;
}

.rating-btn {
  min-width: 40px;
  transition: transform 0.2s ease;
}

.rating-btn:hover {
  transform: scale(1.1);
}

.rating-btn-enhanced {
  min-width: 60px;
  min-height: 60px;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.rating-btn-enhanced:hover {
  transform: scale(1.15);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.rating-btn-selected {
  border: 2px solid var(--q-primary) !important;
  box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.2);
}

.rating-section {
  border: 1px solid var(--q-border-color);
  background-color: var(--q-card);
  transition: all 0.2s ease;
}

.rating-section:hover {
  border-color: var(--q-primary);
  box-shadow: 0 2px 8px rgba(25, 118, 210, 0.1);
}

.scroll-to-top-btn {
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.scroll-to-top-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
}

.sorting-select {
  min-width: 200px;
}

.rating-score {
  display: flex;
  align-items: center;
  gap: 4px;
}

.dialog-rating {
  margin: 0 8px;
}

.imdb-logo {
  width: 32px;
  height: 16px;
}

.tmdb-logo {
  width: 32px;
  height: 13px;
}

.score-text {
  font-size: 0.9rem;
  font-weight: 500;
}
</style>
