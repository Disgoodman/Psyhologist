<template>
  <vue-final-modal class="modal-container"
                   content-class="modal-content"
                   content-transition="vfm-fade"
                   overlay-transition="vfm-fade">
    <div class="modal--header">
      <p class="modal-title">
        <span>Расписание {{ date.toFormat('dd.MM.yyyy') }}</span>
      </p>
      <button class="modal-close" @click="emit('cancel')">
        <svg focusable="false" width="2em" height="2em" viewBox="0 0 24 24">
          <use href="#icon-x"/>
        </svg>
      </button>
    </div>
    <div class="modal--content">
      <table class="table">
        <thead>
        <tr>
          <th scope="col">Начало работы</th>
          <th scope="col">Конец работы</th>
          <th scope="col">Перерыв</th>
        </tr>
        </thead>
        <tbody>
        <tr>
          <td><input type="time" v-model="data.startTime" step="3600"/></td>
          <td><input type="time" v-model="data.endTime" step="3600"/></td>
          <td><input type="time" v-model="data.breakTime" step="3600"/></td>
        </tr>
        </tbody>
      </table>
    </div>
    <div class="modal--action">
      <button @click="submit">Изменить</button>
      <button @click="remove">Удалить</button>
    </div>
  </vue-final-modal>
</template>

<script setup>
import { ref, watch } from "vue";
import { VueFinalModal } from 'vue-final-modal'
import { DateTime } from "luxon";

const emit = defineEmits(['submit', 'remove', 'cancel'])
const props = defineProps({
  date: DateTime,
  schedule: Object,
})

const data = ref({});
watch(() => props.schedule, v => data.value = {
  startTime: v.startTime.toFormat('HH:mm'),
  endTime: v.endTime.toFormat('HH:mm'),
  breakTime: v.breakTime.toFormat('HH:mm'),
}, { immediate: true })

const getFormattedData = () => ({
  date: props.date.toISODate(),
  startTime: data.value.startTime,
  endTime: data.value.endTime,
  breakTime: data.value.breakTime,
});

const submit = () => emit('submit', getFormattedData());
const remove = () => emit('remove', { date: props.date.toISODate() });

</script>

<style scoped>

</style>
