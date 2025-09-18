<template>
  <q-page class="q-pa-md">
    <!-- Loading state -->
    <div v-if="loading" class="row justify-center q-pa-lg">
      <q-spinner-dots size="50px" color="primary" />
    </div>

    <!-- Group details form -->
    <div v-else-if="group" class="row justify-center">
      <div class="col-12">
        <q-card class="q-pa-md">
          <!-- Group Header -->
          <q-card-section>
            <div class="text-h5 q-mb-md">{{ group.name }}</div>
          </q-card-section>

          <!-- Non-member view: Only show basic info and join button -->
          <div v-if="!canEditGroup">
            <q-card-section class="text-center">
              <!-- Show login button if user is not authenticated -->
              <q-btn
                v-if="!authStore.isAuthenticated"
                color="primary"
                icon="img:https://developers.google.com/identity/images/g-logo.png"
                :label="$t('groups.joinNow')"
                @click="loginWithGoogleAndJoin"
                size="md"
              />
              <!-- Show join group button if user is authenticated but not a member -->
              <q-btn
                v-else
                color="primary"
                icon="group_add"
                :label="$t('groups.joinGroup')"
                :loading="saving"
                @click="joinGroup"
                size="lg"
              />
            </q-card-section>
          </div>

          <!-- Member/Creator view: Tabs -->
          <div v-else>
            <!-- Desktop: Show all tabs (750px and above) -->
            <q-tabs
              v-if="$q.screen.width >= 500"
              v-model="activeTab"
              dense
              class="text-grey"
              active-color="primary"
              indicator-color="primary"
              align="justify"
              narrow-indicator
            >
              <q-tab
                name="rating"
                :label="$t('groups.tabs.rating')"
                icon="star"
              />
              <q-tab
                name="recommendations"
                :label="$t('groups.tabs.recommendations')"
                icon="recommend"
              />
              <q-tab name="settings" :label="$t('groups.tabs.settings')" icon="settings" />
              <q-tab
                v-if="group.isCreator"
                name="approvals"
                :label="$t('groups.tabs.approvals')"
                icon="how_to_reg"
              />
            </q-tabs>

            <!-- Mobile: Show primary tabs + dropdown for settings/members -->
            <div v-else class="row items-center">
              <q-tabs
                v-model="activeTab"
                dense
                class="text-grey col"
                active-color="primary"
                indicator-color="primary"
                narrow-indicator
              >
                <q-tab
                  name="rating"
                  :label="$t('groups.tabs.rating')"
                  icon="star"
                />
                <q-tab
                  name="recommendations"
                  :label="$t('groups.tabs.recommendations')"
                  icon="recommend"
                />
              </q-tabs>

              <!-- Dropdown menu for additional tabs -->
              <q-btn-dropdown
                flat
                class="q-ml-sm"
              >
                <q-list dense>
                  <q-item
                    clickable
                    @click="activeTab = 'settings'"
                    :class="{ 'bg-primary text-white': activeTab === 'settings' }"
                    v-close-popup
                  >
                    <q-item-section avatar>
                      <q-icon name="settings" />
                    </q-item-section>
                    <q-item-section>
                      <q-item-label>{{ $t('groups.tabs.settings') }}</q-item-label>
                    </q-item-section>
                  </q-item>

                  <q-item
                    v-if="group.isCreator"
                    clickable
                    @click="activeTab = 'approvals'"
                    :class="{ 'bg-primary text-white': activeTab === 'approvals' }"
                    v-close-popup
                  >
                    <q-item-section avatar>
                      <q-icon name="how_to_reg" />
                    </q-item-section>
                    <q-item-section>
                      <q-item-label>{{ $t('groups.tabs.approvals') }}</q-item-label>
                    </q-item-section>
                  </q-item>
                </q-list>
              </q-btn-dropdown>
            </div>

            <q-separator />

            <q-tab-panels v-model="activeTab" animated>
              <!-- Rating Tab -->
              <q-tab-panel name="rating">
                <!-- Loading content -->
                <div v-if="loadingContent" class="text-center q-pa-lg">
                  <q-spinner-dots size="30px" color="primary" />
                  <div class="text-body2 q-mt-md">{{ $t('groups.rating.loading') }}</div>
                </div>

                <!-- Content items to rate -->
                <div v-else-if="currentContentItem" class="q-pa-xs rating-content-container">
                  <div class="text-center q-mb-md">
                    <div ref="contentTitleRef" class="text-h6 q-mb-xs">{{ currentContentItem.title }}</div>

                    <!-- Content Type and Year -->
                    <div class="text-body2 text-grey-5 q-mb-xs">
                      <span v-if="currentContentType">{{ currentContentType }}</span>
                      <span v-if="currentContentType && currentContentItem.releaseYear"> • </span>
                      <span v-if="currentContentItem.releaseYear">{{ currentContentItem.releaseYear }}</span>
                      <span v-if="(currentContentType || currentContentItem.releaseYear) && currentContentItem.languages && currentContentItem.languages.length > 0"> • </span>
                      <span v-if="currentContentItem.languages && currentContentItem.languages.length > 0">
                        {{ currentContentItem.languages.map(lang => getTranslatedLanguageName(lang)).join(', ') }}
                      </span>
                    </div>

                    <!-- Content Image -->
                    <div v-if="currentContentItem.imageUrl" class="q-mb-md">
                      <q-img
                        :src="currentContentItem.imageUrl"
                        :alt="currentContentItem.title"
                        style="max-width: 300px; max-height: 400px"
                        class="rounded-borders"
                        loading="lazy"
                      >
                        <template v-slot:error>
                          <div class="absolute-full flex flex-center bg-grey-3">
                            <q-icon name="broken_image" size="50px" color="grey-6" />
                          </div>
                        </template>
                      </q-img>
                    </div>

                    <!-- Content Description -->
                    <div
                      v-if="hasDescription"
                      class="text-body2 text-grey-4 q-mb-md"
                    >
                      <div class="text-left q-pa-sm bg-dark rounded-borders description-container">
                        <div
                          ref="descriptionContent"
                          class="description-text"
                          :class="{
                            'description-collapsed': !descriptionExpanded,
                            'has-overflow': !descriptionExpanded && hasOverflow
                          }"
                          :style="{ maxHeight: descriptionExpanded ? 'none' : DESCRIPTION_MAX_HEIGHT }"
                        >
                          {{ fullDescription }}
                        </div>

                        <!-- Show more/less button -->
                        <div v-if="hasOverflow" class="q-mt-sm">
                          <q-btn
                            flat
                            dense
                            size="sm"
                            :label="descriptionExpanded ? $t('common.showLess') : $t('common.showMore')"
                            @click="descriptionExpanded = !descriptionExpanded"
                            color="primary"
                            class="q-pa-none show-more-btn"
                          />
                        </div>
                      </div>
                    </div>

                    <!-- Cast -->
                    <div
                      v-if="currentContentItem.cast && currentContentItem.cast.length > 0"
                      class="text-body2 text-grey-4 q-mb-md"
                    >
                      <div class="text-left q-pa-sm bg-dark rounded-borders">
                        <div class="text-subtitle2 text-grey-3 q-mb-xs">
                          {{ $t('content.dialog.cast') }}
                        </div>
                        <div class="cast-list">
                          {{ currentContentItem.cast.join(', ') }}
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Floating Rating Section -->
                  <div class="floating-rating-container">
                    <!-- Rating title -->
                    <div class="text-center q-mb-sm">
                      <div class="text-subtitle1 text-weight-medium">{{ $t('groups.rating.rateContent') }}</div>
                    </div>

                    <!-- Rating buttons -->
                    <div class="row justify-center items-center">
                      <template v-for="(rating, index) in ratingOptions" :key="`rating-${index}`">
                        <q-btn
                          v-if="rating.value !== 3"
                          :color="selectedRating === rating.value ? 'primary' : 'grey-4'"
                          :text-color="selectedRating === rating.value ? 'white' : 'grey-8'"
                          :outline="selectedRating !== rating.value"
                          :size="$q.screen.lt.md ? 'lg' : 'xl'"
                          :loading="submittingRating && selectedRating === rating.value"
                          round
                          @click="selectAndSubmitRating(rating.value)"
                          class="rating-btn q-mx-xs"
                        >
                          <span :style="$q.screen.lt.md ? 'font-size: 2em' : 'font-size: 2.5em'">{{ rating.emoji }}</span>
                        </q-btn>
                        <!-- Add spacing between 1-2 and 4-5 -->
                        <div v-if="rating.value === 2" class="q-mx-sm"></div>
                      </template>
                    </div>
                  </div>
                </div>

                <!-- No content state -->
                <div v-else class="text-center q-pa-lg">
                  <q-icon name="star_outline" size="4rem" color="grey-5" />
                  <div class="text-h6 q-mt-md text-grey-6">{{ $t('groups.rating.empty.title') }}</div>
                  <div class="text-body2 text-grey-5 q-mb-md">
                    {{ $t('groups.rating.empty.subtitle') }}
                  </div>
                  <q-btn
                    color="primary"
                    :label="$t('groups.rating.loadContent')"
                    @click="fetchContentItems"
                    :loading="loadingContent"
                  />
                </div>
              </q-tab-panel>

              <!-- Recommendations Tab -->
              <q-tab-panel name="recommendations" class="q-pa-none">
                <!-- Content Type Filter (only show if group has both movies and series) -->
                <div v-if="shouldShowContentTypeFilter" class="q-pa-md">
                  <div class="text-subtitle2 q-mb-sm">{{ $t('groups.recommendations.filterByType') }}</div>
                  <div class="row q-gutter-sm q-mb-md">
                    <q-chip
                      v-for="contentType in contentTypes"
                      :key="contentType.value"
                      :selected="suggestionsContentTypeFilter.includes(contentType.value)"
                      clickable
                      color="accent"
                      text-color="white"
                      @click="toggleContentTypeFilter(contentType.value); onContentTypeFilterChange()"
                    >
                      <q-icon :name="contentType.icon" class="q-mr-xs" />
                      {{ contentType.label }}
                    </q-chip>
                  </div>
                </div>

                <!-- Loading state -->
                <div v-if="suggestionsLoading" class="row justify-center q-pa-lg">
                  <q-spinner-dots size="40px" color="primary" />
                </div>

                <!-- Suggestions list using GroupSuggestionCard -->
                <div v-else-if="filteredSuggestions.length > 0" class="q-px-sm q-pt-sm">
                  <div class="row">
                    <div
                      v-for="(suggestion, index) in filteredSuggestions"
                      :key="suggestion.id || `suggestion-${index}`"
                      class="col-6 col-md-3 col-lg-2 q-pa-xs"
                    >
                      <GroupSuggestionCard
                        :suggestion="suggestion"
                        @view-content="onViewContent"
                        class="suggestion-card"
                      />
                    </div>
                  </div>

                  <!-- Load More Button -->
                  <div v-if="hasMoreSuggestions" class="row justify-center q-mt-lg">
                    <q-btn
                      color="primary"
                      :label="$t('groups.recommendations.loadMore')"
                      :loading="loadingMoreSuggestions"
                      @click="loadMoreSuggestions"
                      icon="expand_more"
                      outline
                    />
                  </div>

                  <!-- Show total count -->
                  <div class="text-center q-mt-md text-grey-6">
                    {{ $t('groups.recommendations.showing', {
                      current: filteredSuggestions.length,
                      total: suggestionsContentTypeFilter.length > 0 ? filteredSuggestions.length : suggestionsTotalCount
                    }) }}
                  </div>
                </div>

                <!-- Empty state -->
                <div v-else class="row justify-center q-pa-lg">
                  <div class="text-center">
                    <q-icon name="lightbulb_outline" size="3rem" color="grey-5" />
                    <div class="text-h6 q-mt-md text-grey-6">
                      {{ $t('groups.recommendations.empty.title') }}
                    </div>
                    <div class="text-body2 text-grey-5">
                      {{ $t('groups.recommendations.empty.description') }}
                    </div>
                  </div>
                </div>
              </q-tab-panel>

              <!-- Member Approvals Tab -->
              <q-tab-panel v-if="group.isCreator" name="approvals">
                <div class="text-body2 text-grey-6 q-mb-lg">
                  {{ $t('groups.approvals.description') }}
                </div>

                <!-- Share Link Section -->
                <div v-if="group.shareLink" class="q-mb-lg">
                  <div class="text-subtitle2 q-mb-sm">
                    {{ $t('groups.editDialog.shareLink') }}
                  </div>
                  <div class="row items-center q-gutter-sm">
                    <q-input :model-value="group.shareLink" readonly filled dense class="col" />
                    <q-btn
                      color="info"
                      icon="content_copy"
                      :label="$t('groups.copyShareLink')"
                      @click="copyShareLink(group.shareLink)"
                      dense
                    />
                  </div>
                </div>

                <!-- Pending and approved members list -->
                <div v-if="membersToApprove.length > 0" class="q-gutter-md">
                  <q-card
                    v-for="member in membersToApprove"
                    :key="`member-${member.id}`"
                    class="q-pa-md"
                    flat
                    bordered
                  >
                    <div class="row items-center justify-between">
                      <div class="col">
                        <div class="text-subtitle1">
                          {{ member.name || member.email }}
                          <q-chip
                            :color="
                              member.status === GroupMemberStatus.Pending ? 'orange' : 'green'
                            "
                            :label="
                              member.status === GroupMemberStatus.Pending ? 'Pending' : 'Approved'
                            "
                            size="sm"
                            class="q-ml-sm"
                          />
                        </div>
                        <div v-if="member.name" class="text-caption text-grey-6">
                          {{ member.email }}
                        </div>
                      </div>
                      <div class="col-auto">
                        <q-btn
                          v-if="member.status === GroupMemberStatus.Pending"
                          color="positive"
                          icon="check"
                          :label="$t('groups.approvals.approve')"
                          @click="approveMember(member.id)"
                          :loading="approvingMembers.includes(member.id!)"
                          class="q-mr-sm"
                          dense
                        />
                        <q-btn
                          v-if="
                            (member.status === GroupMemberStatus.Pending ||
                              member.status === GroupMemberStatus.Approved) &&
                            group.creatorId !== member.id
                          "
                          color="negative"
                          icon="close"
                          :label="$t('groups.approvals.reject')"
                          @click="rejectMember(member.id)"
                          :loading="rejectingMembers.includes(member.id!)"
                          dense
                          flat
                        />
                      </div>
                    </div>
                  </q-card>
                </div>

                <!-- Empty state -->
                <div v-else class="row justify-center q-pa-lg">
                  <div class="text-center">
                    <q-icon name="people_outline" size="3rem" color="grey-5" />
                    <div class="text-h6 q-mt-md text-grey-6">
                      {{ $t('groups.approvals.empty.title') }}
                    </div>
                    <div class="text-body2 text-grey-5">
                      {{ $t('groups.approvals.empty.description') }}
                    </div>
                  </div>
                </div>
              </q-tab-panel>

              <!-- Settings Tab -->
              <q-tab-panel name="settings">
                <div class="text-h6 q-mb-md">
                  {{
                    group.isCreator ? $t('groups.editDialog.title') : $t('groups.viewDialog.title')
                  }}
                </div>

                <q-input
                  v-model="groupForm.name"
                  :label="$t('groups.createDialog.nameLabel')"
                  :rules="[(val) => !!val || $t('groups.createDialog.nameRequired')]"
                  :readonly="!group.isCreator"
                  filled
                  class="q-mb-md"
                />

                <div class="text-subtitle2 q-mb-sm">
                  {{ $t('groups.createDialog.servicesLabel') }}
                </div>
                <div class="row q-gutter-sm q-mb-md">
                  <q-chip
                    v-for="(service, index) in availableServices"
                    :key="service.id || service.name || `service-${index}`"
                    :selected="selectedServices.includes(service.id || '')"
                    :clickable="group.isCreator"
                    color="primary"
                    text-color="white"
                    @click="group.isCreator ? toggleService(service.id || '') : null"
                  >
                    {{ service.name }}
                  </q-chip>
                </div>

                <div class="text-subtitle2 q-mb-sm">
                  {{ $t('groups.createDialog.contentTypesLabel') }}
                </div>
                <div class="row q-gutter-sm q-mb-md">
                  <q-chip
                    v-for="contentType in contentTypes"
                    :key="contentType.value"
                    :selected="selectedContentTypes.includes(contentType.value)"
                    :clickable="group.isCreator"
                    color="accent"
                    text-color="white"
                    @click="group.isCreator ? toggleContentType(contentType.value) : null"
                  >
                    <q-icon :name="contentType.icon" class="q-mr-xs" />
                    {{ contentType.label }}
                  </q-chip>
                </div>

                <div class="text-subtitle2 q-mb-sm">
                  {{ $t('groups.createDialog.categoriesLabel') }}
                </div>
                <div class="row q-gutter-sm q-mb-md">
                  <q-chip
                    v-for="(category, index) in availableCategories"
                    :key="category.id || category.name || `category-${index}`"
                    :selected="selectedCategories.includes(category.id || '')"
                    :clickable="group.isCreator"
                    color="secondary"
                    text-color="white"
                    @click="group.isCreator ? toggleCategory(category.id || '') : null"
                  >
                    {{ category.name }}
                  </q-chip>
                </div>

                <div v-if="group.isCreator" class="text-subtitle2 q-mb-sm">
                  {{ $t('groups.editDialog.membershipSettings') }}
                </div>
                <q-toggle
                  v-if="group.isCreator"
                  v-model="requireApproval"
                  :label="$t('groups.editDialog.requireApproval')"
                  color="primary"
                  class="q-mb-md"
                />

                <!-- Card Actions within Settings Tab -->
                <div class="q-mt-lg">
                  <div class="row justify-end q-gutter-sm">
                    <q-btn flat :label="$t('common.cancel')" @click="goBack" />
                    <q-btn
                      v-if="group.isCreator"
                      color="primary"
                      :label="$t('groups.save')"
                      :loading="saving"
                      @click="saveGroup"
                    />
                  </div>
                </div>

                <!-- Danger Zone for Creators within Settings Tab -->
                <div v-if="group.isCreator" class="danger-zone q-mt-lg">
                  <q-separator class="q-mb-md" />
                  <div class="text-subtitle2 text-negative q-mb-sm">
                    <q-icon name="warning" class="q-mr-xs" />
                    {{ $t('groups.dangerZone.title') }}
                  </div>
                  <div class="text-body2 text-grey-6 q-mb-md">
                    {{ $t('groups.dangerZone.description') }}
                  </div>
                  <q-btn
                    flat
                    color="negative"
                    icon="delete"
                    :label="$t('groups.delete')"
                    @click="confirmDeleteGroup"
                    class="danger-btn"
                  />
                </div>
              </q-tab-panel>
            </q-tab-panels>
          </div>

          <!-- Non-member action buttons -->
          <q-card-actions v-if="!canEditGroup && authStore.isAuthenticated" align="right">
            <q-btn flat :label="$t('common.back')" @click="goBack" />
          </q-card-actions>
        </q-card>
      </div>
    </div>

    <!-- Error state -->
    <div v-else class="row justify-center q-pa-lg">
      <div class="text-center">
        <q-icon name="error" size="4rem" color="negative" />
        <div class="text-h6 q-mt-md text-negative">{{ $t('groups.errors.notFound') }}</div>
        <q-btn color="primary" :label="$t('groups.backToList')" @click="goBack" class="q-mt-md" />
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from 'src/stores/auth-store';
import GroupSuggestionCard from 'components/GroupSuggestionCard.vue';
import {
  Client,
  type GroupResponse,
  GroupRequest,
  GroupSettings,
  type ServiceResponse,
  type CategoryResponse,
  type GroupSuggestion,
  type GroupSuggestionsResponse,
  GroupMemberStatus,
  ChangeMemberStateRequest,
  type ContentResponse,
  ReviewRequest,
} from 'src/api/client';

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const group = ref<GroupResponse | null>(null);
const loading = ref(false);
const saving = ref(false);
const activeTab = ref('rating');
const suggestions = ref<GroupSuggestion[]>([]);
const suggestionsLoading = ref(false);
const loadingMoreSuggestions = ref(false);
const suggestionsOffset = ref(0);
const suggestionsTotalCount = ref(0);
const suggestionsContentTypeFilter = ref<number[]>([]); // Filter for content types (1=movie, 2=series, empty=all)
const approvingMembers = ref<string[]>([]);
const rejectingMembers = ref<string[]>([]);
const selectedServices = ref<string[]>([]);
const selectedCategories = ref<string[]>([]);
const selectedContentTypes = ref<number[]>([]);
const requireApproval = ref(false);
const availableServices = ref<ServiceResponse[]>([]);
const availableCategories = ref<CategoryResponse[]>([]);

// Login form state
const loggingIn = ref(false);
const loginError = ref<string | null>(null);

// Content rating state
const contentItems = ref<ContentResponse[]>([]);
const currentItemIndex = ref(0);
const selectedRating = ref<number | null>(null);
const loadingContent = ref(false);
const submittingRating = ref(false);
const descriptionExpanded = ref(false);
const descriptionContent = ref<HTMLElement | null>(null);
const contentTitleRef = ref<HTMLElement | null>(null);

const client = new Client();

// Constants
const DESCRIPTION_MAX_HEIGHT = '300px'; // Visible height before expansion

// Rating options with emojis
const ratingOptions = [
  { value: 1, emoji: '😞' }, // Very sad/disappointed
  { value: 2, emoji: '😕' }, // Slightly disappointed
  { value: 3, emoji: '😐' }, // Neutral/okay
  { value: 4, emoji: '😊' }, // Happy/good
  { value: 5, emoji: '😍' }, // Love it/excellent
];

const groupId = computed(() => route.params.id as string);

// Check if user can edit the group (is member or creator)
const canEditGroup = computed(() => {
  return group.value?.isMember || group.value?.isCreator;
});

// Content types options (1=movie, 2=series)
const contentTypes = computed(() => [
  { value: 1, label: t('groups.contentTypes.movie'), icon: 'movie' },
  { value: 2, label: t('groups.contentTypes.series'), icon: 'tv' },
]);

// Get pending and approved members from group data
const membersToApprove = computed(() => {
  if (!group.value?.members) return [];
  const pending = group.value.members.filter(
    (member) => member.status === GroupMemberStatus.Pending,
  );
  const approved = group.value.members.filter(
    (member) => member.status === GroupMemberStatus.Approved,
  );
  return [...pending, ...approved];
});

// Computed property for current content item
const currentContentItem = computed(() => {
  if (contentItems.value.length === 0 || currentItemIndex.value >= contentItems.value.length) {
    return null;
  }
  return contentItems.value[currentItemIndex.value];
});

// Computed properties for description handling
const hasDescription = computed(() => {
  return !!currentContentItem.value?.shortDescription;
});

const fullDescription = computed(() => {
  return currentContentItem.value?.shortDescription || '';
});

// Get content type label
const currentContentType = computed(() => {
  if (!currentContentItem.value?.type) return '';
  const typeOption = contentTypes.value.find(ct => ct.value === currentContentItem.value?.type);
  return typeOption?.label || '';
});

// Check if content overflows the max height
const hasOverflow = ref(false);

// Check if there are more suggestions to load
const hasMoreSuggestions = computed(() => {
  return suggestions.value.length < suggestionsTotalCount.value;
});

// Check if group has both content types selected (to show filter)
const shouldShowContentTypeFilter = computed(() => {
  const groupContentTypes = group.value?.settings?.contentTypes || [];
  // Show filter if both types are selected OR neither is selected (all content accepted)
  return (groupContentTypes.includes(1) && groupContentTypes.includes(2)) ||
         groupContentTypes.length === 0;
});

// Filtered suggestions based on content type filter
const filteredSuggestions = computed(() => {
  if (suggestionsContentTypeFilter.value.length === 0) {
    return suggestions.value;
  }
  return suggestions.value.filter(suggestion =>
    suggestion.type && suggestionsContentTypeFilter.value.includes(suggestion.type)
  );
});

const checkOverflow = () => {
  if (!descriptionContent.value) return;

  // Temporarily expand to get full height
  const originalMaxHeight = descriptionContent.value.style.maxHeight;
  descriptionContent.value.style.maxHeight = 'none';

  const fullHeight = descriptionContent.value.scrollHeight;
  const maxHeightPx = parseInt(DESCRIPTION_MAX_HEIGHT);

  hasOverflow.value = fullHeight > maxHeightPx;

  // Restore original max height if collapsed
  if (!descriptionExpanded.value) {
    descriptionContent.value.style.maxHeight = originalMaxHeight;
  }
};

const scrollToTitle = () => {
  if (contentTitleRef.value) {
    contentTitleRef.value.scrollIntoView({
      behavior: 'smooth',
      block: 'start'
    });
  }
};

const groupForm = ref({
  name: '',
});

const fetchGroup = async () => {
  if (!groupId.value) return;

  loading.value = true;
  try {
    const foundGroup = await client.getGroup(groupId.value);

    if (foundGroup) {
      group.value = foundGroup;
      groupForm.value.name = foundGroup.name || '';

      // Settings already contain IDs, use them directly
      selectedServices.value = foundGroup.settings?.services || [];
      selectedCategories.value = foundGroup.settings?.categories || [];
      selectedContentTypes.value = foundGroup.settings?.contentTypes || [];
      requireApproval.value = foundGroup.requireApproval || false;
    } else {
      group.value = null;
    }
  } catch (error) {
    console.error('Error fetching group:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.fetchFailed'),
    });
    group.value = null;
  } finally {
    loading.value = false;
  }
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
    // Fallback to empty array if services can't be loaded
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
    // Fallback to empty array if categories can't be loaded
    availableCategories.value = [];
  }
};

const fetchSuggestions = async (reset = true) => {
  if (!groupId.value) return;

  if (reset) {
    suggestionsLoading.value = true;
    suggestionsOffset.value = 0;
    suggestions.value = [];
  } else {
    loadingMoreSuggestions.value = true;
  }

  const pageSize = 24;

  try {
    const response: GroupSuggestionsResponse = await client.getGroupSuggestions(
      groupId.value,
      pageSize,
      suggestionsOffset.value
    );

    const newSuggestions = response.items || [];
    suggestionsTotalCount.value = response.totalCount || 0;

    if (reset) {
      suggestions.value = newSuggestions;
    } else {
      suggestions.value = [...suggestions.value, ...newSuggestions];
    }

    suggestionsOffset.value += newSuggestions.length;
  } catch (error) {
    console.error('Error fetching suggestions:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.suggestionsFetchFailed'),
    });
    if (reset) {
      suggestions.value = [];
    }
  } finally {
    suggestionsLoading.value = false;
    loadingMoreSuggestions.value = false;
  }
};

const loadMoreSuggestions = async () => {
  await fetchSuggestions(false);
};

const onContentTypeFilterChange = () => {
  // Filter is handled by computed property, no additional action needed
  // Could be extended to save filter preference or trigger analytics
};

const saveGroup = async () => {
  if (!group.value?.id || !groupForm.value.name.trim()) return;

  saving.value = true;
  try {
    const groupSettings = new GroupSettings();

    // Settings expect IDs, use them directly
    if (selectedServices.value.length > 0) {
      groupSettings.services = selectedServices.value;
    }

    if (selectedCategories.value.length > 0) {
      groupSettings.categories = selectedCategories.value;
    }

    if (selectedContentTypes.value.length > 0) {
      groupSettings.contentTypes = selectedContentTypes.value;
    }

    const groupRequest = new GroupRequest({
      name: groupForm.value.name.trim(),
      settings: groupSettings,
      requireApproval: requireApproval.value,
    });

    await client.updateGroup(group.value.id, groupRequest);

    $q.notify({
      type: 'positive',
      message: t('groups.updateSuccess'),
    });

    // Navigate back to view page after successful update
    void router.push(`/groups`);
  } catch (error) {
    console.error('Error updating group:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.updateFailed'),
    });
  } finally {
    saving.value = false;
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

const toggleContentTypeFilter = (contentType: number) => {
  const index = suggestionsContentTypeFilter.value.indexOf(contentType);
  if (index > -1) {
    suggestionsContentTypeFilter.value.splice(index, 1);
  } else {
    suggestionsContentTypeFilter.value.push(contentType);
  }
};

const goBack = () => {
  void router.push('/groups');
};


const loginWithGoogleAndJoin = async () => {
  loggingIn.value = true;
  loginError.value = null;

  try {
    await authStore.loginUserWithGoogle();

    // After successful login, join the group
    await joinGroup();


    $q.notify({
      type: 'positive',
      message: t('auth.loginSuccess'),
    });
  } catch (error) {
    console.error('Google login error:', error);
    loginError.value = error instanceof Error ? error.message : t('auth.loginFailed');
  } finally {
    loggingIn.value = false;
  }
};

const copyShareLink = async (shareLink: string) => {
  try {
    await navigator.clipboard.writeText(shareLink);
    $q.notify({
      type: 'positive',
      message: t('groups.shareLinkCopied'),
      icon: 'content_copy',
    });
  } catch (error) {
    console.error('Error copying share link:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.copyFailed'),
    });
  }
};

const joinGroup = async () => {
  if (!group.value?.id) return;

  saving.value = true;
  try {
    await client.joinGroup(group.value.id);

    $q.notify({
      type: 'positive',
      message: t('groups.joinSuccess'),
    });

    // Refresh group data to update membership status
    await fetchGroup();
  } catch (error) {
    console.error('Error joining group:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.joinFailed'),
    });
  } finally {
    saving.value = false;
  }
};

const confirmDeleteGroup = () => {
  if (!group.value) return;

  $q.dialog({
    title: t('groups.deleteDialog.title'),
    message: t('groups.deleteDialog.message', { name: group.value.name }),
    cancel: true,
    persistent: true,
  }).onOk(() => {
    void deleteGroup();
  });
};

const deleteGroup = async () => {
  if (!group.value?.id) return;

  saving.value = true;
  try {
    await client.deleteGroup(group.value.id);

    $q.notify({
      type: 'positive',
      message: t('groups.deleteSuccess'),
    });

    // Navigate back to groups list after successful deletion
    void router.push('/groups');
  } catch (error) {
    console.error('Error deleting group:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.deleteFailed'),
    });
  } finally {
    saving.value = false;
  }
};

const onViewContent = (suggestion: GroupSuggestion) => {
  // Handle viewing content details
  // This could navigate to a content details page or open a modal
  console.log('View content:', suggestion);
  // TODO: Implement content viewing functionality
};

const changeMemberState = async (memberId: string | undefined, state: GroupMemberStatus) => {
  if (!groupId.value || !group.value || !memberId) return;

  const isApproving = state === GroupMemberStatus.Approved;
  const loadingArray = isApproving ? approvingMembers : rejectingMembers;

  loadingArray.value.push(memberId);

  try {
    const request = new ChangeMemberStateRequest({ state });
    await client.changeMemberState(groupId.value, memberId, request);

    $q.notify({
      type: 'positive',
      message: isApproving
        ? t('groups.approvals.approveSuccess')
        : t('groups.approvals.rejectSuccess'),
    });

    // Update the group data based on state
    if (group.value.members) {
      if (state === GroupMemberStatus.Approved) {
        // Update the member status to approved
        const member = group.value.members.find((m) => m.id === memberId);
        if (member) {
          member.isApproved = true;
        }
      } else {
        // Remove the member from the group data for rejection
        group.value.members = group.value.members.filter((m) => m.id !== memberId);
      }
    }
  } catch (error) {
    console.error(`Error ${state.toLowerCase()}ing member:`, error);
    $q.notify({
      type: 'negative',
      message: isApproving ? t('groups.errors.approveFailed') : t('groups.errors.rejectFailed'),
    });
  } finally {
    loadingArray.value = loadingArray.value.filter((id) => id !== memberId);
  }
};

// Convenience functions for backward compatibility and cleaner template usage
const approveMember = (memberId: string | undefined) =>
  changeMemberState(memberId, GroupMemberStatus.Approved);
const rejectMember = (memberId: string | undefined) =>
  changeMemberState(memberId, GroupMemberStatus.Rejected);

// Language translation helper
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

// Rating functions
const fetchContentItems = async () => {
  if (!group.value?.id) return;

  loadingContent.value = true;
  try {
    const content = await client.getGroupContent(group.value.id);
    contentItems.value = content || [];
    currentItemIndex.value = 0;
    selectedRating.value = null;
    descriptionExpanded.value = false; // Reset expanded state
    hasOverflow.value = false; // Reset overflow detection
  } catch (error) {
    console.error('Error fetching content:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.contentFetchFailed'),
    });
    contentItems.value = [];
  } finally {
    loadingContent.value = false;
  }
};

const selectAndSubmitRating = async (rating: number) => {
  if (!currentContentItem.value?.id || !group.value?.id) return;

  selectedRating.value = rating;
  submittingRating.value = true;

  try {
    const reviewRequest = new ReviewRequest({
      groupId: group.value.id,
      contentId: currentContentItem.value.id,
      rating: rating,
      type: 1, // Assuming 1 represents rating type
    });

    await client.reviewContent(currentContentItem.value.id, reviewRequest);

    // Move to next item or fetch more content
    if (currentItemIndex.value < contentItems.value.length - 1) {
      currentItemIndex.value++;
      selectedRating.value = null;
      descriptionExpanded.value = false; // Reset expanded state for new item
      hasOverflow.value = false; // Reset overflow detection for new item

      // Scroll to title after content changes
      void nextTick(() => {
        scrollToTitle();
      });
    } else {
      // All items reviewed, fetch more
      await fetchContentItems();

      // Scroll to title after new content is loaded
      void nextTick(() => {
        scrollToTitle();
      });
    }
  } catch (error) {
    console.error('Error submitting rating:', error);
    $q.notify({
      type: 'negative',
      message: t('groups.errors.ratingSubmitFailed'),
    });
  } finally {
    submittingRating.value = false;
  }
};

onMounted(() => {
  // Load all data in parallel since no mapping is needed
  void Promise.all([fetchServices(), fetchCategories(), fetchGroup()]);
});

// Watch for tab changes to load data when tabs are accessed
watch(activeTab, (newTab) => {
  if (newTab === 'rating' && canEditGroup.value && contentItems.value.length === 0) {
    void fetchContentItems();
  } else if (newTab === 'recommendations' && canEditGroup.value && suggestions.value.length === 0) {
    void fetchSuggestions(true);
  }
  // No need to fetch pending members as they are part of group data
});

// Watch for content changes to check overflow
watch(
  () => currentContentItem.value?.shortDescription,
  () => {
    // Reset state when content changes
    descriptionExpanded.value = false;
    hasOverflow.value = false;

    // Check overflow on next tick after DOM update
    void nextTick(() => {
      checkOverflow();
    });
  },
  { immediate: true }
);

// Watch for group changes to fetch suggestions if recommendations tab is active
watch(
  () => group.value?.id,
  (newGroupId) => {
    if (newGroupId && canEditGroup.value) {
      // Reset content type filter when group changes
      suggestionsContentTypeFilter.value = [];

      if (activeTab.value === 'rating' && contentItems.value.length === 0) {
        void fetchContentItems();
      } else if (activeTab.value === 'recommendations') {
        void fetchSuggestions(true);
      }
    }
  },
);
</script>

<style scoped>
.danger-zone {
  background: rgba(255, 0, 0, 0.05);
  border-radius: 8px;
  margin: 16px;
}

.danger-btn {
  border: 1px solid rgba(255, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.danger-btn:hover {
  background: rgba(255, 0, 0, 0.1);
  border-color: rgba(255, 0, 0, 0.5);
}

.rating-btn {
  transition: transform 0.2s ease;
}

.rating-btn:hover {
  transform: scale(1.1);
}

.rating-btn.q-btn--outline {
  border-width: 2px;
}

.description-container {
  position: relative;
}

.description-text {
  overflow: hidden;
  transition: max-height 0.3s ease;
  line-height: 1.5;
}

.description-collapsed {
  position: relative;
}

.description-collapsed.has-overflow::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 2em;
  background: linear-gradient(transparent, rgba(28, 28, 30, 1));
  pointer-events: none;
}

.show-more-btn {
  position: relative;
  z-index: 1;
}

.rating-content-container {
  padding-bottom: 90px; /* Space for floating rating bar */
}

.floating-rating-container {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-top: 1px solid rgba(0, 0, 0, 0.1);
  padding: 12px 16px;
  z-index: 1000;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.1);
}

/* Dark theme support */
.body--dark .floating-rating-container {
  background: rgba(28, 28, 30, 0.95);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.3);
}
</style>
