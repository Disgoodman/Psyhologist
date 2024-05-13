<template>
  <vue-final-modal class="modal-container"
                   content-class="modal-content"
                   content-transition="vfm-fade"
                   overlay-transition="vfm-fade">
    <div class="modal--header">
      <p class="modal-title">
        <span>Удалить всё расписание {{ startDate.toFormat('dd.MM.yyyy') }} - {{ endDate.toFormat('dd.MM.yyyy') }}</span>
      </p>
      <button class="modal-close" @click="emit('cancel')">
        <svg focusable="false" width="2em" height="2em" viewBox="0 0 24 24">
          <use href="#icon-x"/>
        </svg>
      </button>
    </div>
    <div class="modal--content">
      
    </div>
    <div class="modal--action">
      <button @click="submit">Удалить</button>
    </div>
  </vue-final-modal>
</template>

<script setup>
import { VueFinalModal } from 'vue-final-modal'
import { DateTime } from "luxon";

const emit = defineEmits(['submit', 'cancel'])
const props = defineProps({
  startDate: DateTime,
  endDate: DateTime
})

const submit = () => emit('submit', { 
  startDate: props.startDate.toISODate(),
  endDate: props.endDate.toISODate(),
});

</script>