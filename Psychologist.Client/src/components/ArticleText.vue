<template>
  <p>{{ text }}
    <a class="ms-2 link-secondary"
       @click="expanded = !expanded"
       v-if="isExpandNeeded">{{ expanded ? 'Свернуть' : 'Раскрыть' }}</a>
  </p>
</template>

<script setup>
import { ref, computed } from "vue";

const props = defineProps({
  text: String,
  length: Number
})

const expanded = ref(false);

const text = computed(() => expanded.value ? props.text : truncateString(props.text, props.length));
const isExpandNeeded = computed(() => props.text.length > props.length);

const truncateString = (string = '', maxLength) =>
    string.length > maxLength
        ? `${string.substring(0, maxLength)}…`
        : string

</script>

<style scoped>
a {
  cursor: pointer;
}

p {
  white-space: pre-wrap;
}
</style>