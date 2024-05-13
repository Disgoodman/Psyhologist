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
          <td><input type="time" v-model="schedule.startTime" step="3600"/></td>
          <td><input type="time" v-model="schedule.endTime" step="3600"/></td>
          <td><input type="time" v-model="schedule.breakTime" step="3600"/></td>
        </tr>
        </tbody>
      </table>
    </div>
    <div class="modal--action">
      <button @click="submit">Добавить</button>
    </div>
  </vue-final-modal>
</template>

<script setup>
import { reactive, watch } from "vue";
import { VueFinalModal } from 'vue-final-modal'
import { DateTime } from "luxon";

const emit = defineEmits(['submit', 'cancel'])
const props = defineProps({
  date: DateTime,
})

const schedule = reactive({
  startTime: '09:00',
  endTime: '18:00',
  breakTime: '13:00',
});


const getFormattedData = () => ({
  date: props.date.toISODate(),
  startTime: schedule.startTime,
  endTime: schedule.endTime,
  breakTime: schedule.breakTime,
});

const submit = () => emit('submit', getFormattedData());

</script>