<template>
  <q-item
    clickable
    :tag="isExternalLink ? 'a' : 'div'"
    :target="isExternalLink ? '_blank' : undefined"
    :href="isExternalLink ? link : undefined"
    :to="isExternalLink ? undefined : link"
    :active="isActiveRoute"
    active-class="text-teal"
  >
    <q-item-section v-if="icon" avatar>
      <q-icon :name="icon" />
    </q-item-section>
    <q-item-section>
      <q-item-label>{{ title }}</q-item-label>
    </q-item-section>
  </q-item>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

export interface EssentialLinkProps {
  title: string;
  caption?: string;
  link?: string;
  icon?: string;
}

const props = withDefaults(defineProps<EssentialLinkProps>(), {
  caption: '',
  link: '#',
  icon: '',
});

const route = useRoute();

const isExternalLink = computed(() => {
  return props.link?.startsWith('http') || props.link?.startsWith('mailto:') || props.link === '#';
});

const isActiveRoute = computed(() => {
  if (isExternalLink.value || !props.link) {
    return false;
  }
  return route.path === props.link;
});
</script>

<style scoped>
:deep(.q-item.text-teal) {
  color: #00d8c3 !important;
}

:deep(.q-item.bg-teal-1) {
  background-color: rgba(0, 216, 195, 0.15) !important;
}

:deep(.q-item.text-teal .q-icon) {
  color: #00d8c3 !important;
}
</style>
