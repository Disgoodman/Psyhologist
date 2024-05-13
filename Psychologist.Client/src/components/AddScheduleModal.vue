<template>
  <vue-final-modal class="modal-container"
                   content-class="modal-content"
                   content-transition="vfm-fade"
                   overlay-transition="vfm-fade">
    <div class="modal--header">
      <p class="modal-title">
        <span>Расписание {{ startDate.toFormat('dd.MM.yyyy') }} - {{ endDate.toFormat('dd.MM.yyyy') }}</span>
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
          <th scope="col">День недели</th>
          <th scope="col">Начало работы</th>
          <th scope="col">Конец работы</th>
          <th scope="col">Перерыв</th>
          <th scope="col"></th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="weekday in data">
          <td>{{ weekday.day }}</td>
          <td v-if="!weekday.isWorkday" colspan="3"></td>
          <td v-if="weekday.isWorkday"><input type="time" v-model="weekday.startTime" step="3600"/></td>
          <td v-if="weekday.isWorkday"><input type="time" v-model="weekday.endTime" step="3600"/></td>
          <td v-if="weekday.isWorkday"><input type="time" v-model="weekday.breakTime" step="3600"/></td>
          <td>
            <button class="btn btn-outline-secondary"
                    @click="weekday.isWorkday = !weekday.isWorkday">
              {{ weekday.isWorkday ? 'Выходной день' : 'Рабочий день' }}
            </button>
          </td>
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
  startDate: DateTime,
  endDate: DateTime
})

const weekdays = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота', 'Воскресенье']

const data = reactive(weekdays.map((d, i) => ({
  day: d,
  dayNumber: i,
  startTime: '09:00',
  endTime: '18:00',
  breakTime: '13:00',
  isWorkday: i < 5
})))


function getFormattedData() {
  let result = {
    startDate: props.startDate.toISODate(),
    endDate: props.endDate.toISODate(),
    weekdays: {}
  };
  for (let d of data) {
    if (!d.isWorkday) continue;
    result.weekdays[d.dayNumber] = {
      startTime: d.startTime,
      endTime: d.endTime,
      breakTime: d.breakTime,
    }
  }
  return result;
}

const submit = () => emit('submit', getFormattedData());

</script>