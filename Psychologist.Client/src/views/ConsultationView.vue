<template>
  <div>
    <div class="container-md mt-3">
      <router-link :to="{ name: 'scheduleDay', params: { specialistId: props.specialistId, date: props.datetime.toISODate() } }"
                   class="btn btn-outline-secondary btn-sm px-5">
        Назад
      </router-link>
    </div>
    
    <!-- Consultation view -->
    <div v-if="isLoaded && consultation" class="container-md mt-3">
      <p class="mb-2">Посетитель: {{ getVisitorLabel(consultation.visitor) }}</p>

      <div class="input-group mb-2">
        <span class="input-group-text">Тема</span>
        <input type="text" class="form-control" v-model="consultation.topic">
      </div>

      <div class="form-check mb-2">
        <label class="form-check-label">
          <input class="form-check-input" type="checkbox" v-model="consultation.primary">
          Первичное обследование
        </label>
      </div>

      <div class="form-check mb-2">
        <label class="form-check-label">
          <input class="form-check-input" type="checkbox" v-model="consultation.visitorArrived">
          Клиент прибыл
        </label>
      </div>

      <div class="mb-2">
        <span class="input-text">Тип: {{ getConsultationTypeTitleByName(consultation.type) }}</span>
      </div>

      <template v-if="consultation.type === 'individualConsultation'">
        <div class="input-group mb-2" v-if="isPsychologist">
          <label class="input-group-text">Код обращения</label>
          <select class="form-select" v-model="consultation.requestCode">
            <option v-for="code in requestCodes" :value="code.name">{{ code.name }} - {{ code.title }}</option>
          </select>
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Заметки</label>
          <textarea class="form-control" v-model="consultation.notes"></textarea>
        </div>
      </template>
      <template v-else-if="consultation.type === 'individualWork'">
        <div class="input-group mb-2">
          <span class="input-group-text">Цель</span>
          <input type="text" class="form-control" v-model="consultation.purpose">
        </div>
      </template>
      <template v-else-if="consultation.type === 'diagnosticWork'">
        <div class="input-group mb-2">
          <label class="input-group-text">Код обращения</label>
          <select class="form-select" v-model="consultation.requestCode">
            <option v-for="code in requestCodes" :value="code.name">{{ code.name }} - {{ code.title }}</option>
          </select>
        </div>
        <div class="input-group mb-2">
          <span class="input-group-text">Тема обращения</span>
          <input type="text" class="form-control" v-model="consultation.subject">
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Выявлено в ходе диагностики</label>
          <textarea class="form-control" v-model="consultation.revealed"></textarea>
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Назначено по результатам диагностики</label>
          <textarea class="form-control" v-model="consultation.prescribed"></textarea>
        </div>
      </template>

      <button class="w-100 btn btn-outline-secondary" @click="updateConsultation">Сохранить</button>
      <button class="w-100 btn btn-outline-secondary mt-1" @click="deleteConsultation">Удалить</button>
    </div>

    <!-- New consultation form -->
    <div v-else-if="isLoaded && !consultation" class="container-md mt-3">
      <h3 class="text-center mb-3">Новая консультация</h3>
      <p class="mb-2">Дата и время: {{ datetime?.toFormat("dd.MM.yyyy HH:mm") }}</p>

      <div class="mb-2">
        <label class="typo__label">Посетитель</label>
        <div v-if="!newVisitorFormVisible" class="d-flex">
          <Multiselect v-model="selectedVisitor" :options="visitors" :showNoOptions="true"
                       class="flex-grow-1" track-by="id" :custom-label="getVisitorOptionSearchText"
                       selectLabel="Нажмите чтобы выбрать"
                       selectedLabel="Выбрано"
                       deselectLabel="Нажмите чтобы удалить">
            <template #option="o">{{ getVisitorLabel(o.option) }}</template>
            <template #singleLabel="o">{{ getVisitorLabel(o.option) }}</template>
            <template #noOptions>Список пользователей пуст</template>
            <template #noResult>Пользователей с таким ФИО не найдено</template>
            <template #placeholder>Выберите пользователя</template>
          </Multiselect>
          <button class="ms-2 btn btn-outline-secondary" @click="newVisitorFormVisible = true">Добавить</button>
        </div>

        <div v-if="newVisitorFormVisible" class="card">
          <div class="card-body">
            <div class="input-group mb-3">
              <span class="input-group-text">Имя</span>
              <input type="text" class="form-control" v-model="newVisitor.firstName">
            </div>
            <div class="input-group mb-3">
              <span class="input-group-text">Фамилия</span>
              <input type="text" class="form-control" v-model="newVisitor.lastName">
            </div>
            <div class="input-group mb-3">
              <span class="input-group-text">Отчество</span>
              <input type="text" class="form-control" v-model="newVisitor.patronymic">
            </div>
            <div class="input-group mb-3">
              <span class="input-group-text">Дата рождения</span>
              <input type="date" class="form-control" v-model="newVisitor.birthday">
            </div>
            <div class="input-group mb-3">
              <label class="input-group-text">Тип</label>
              <select class="form-select" v-model="newVisitor.type">
                <option v-for="type in visitorTypes" :value="type.name">{{ type.title }}</option>
              </select>
            </div>

            <div class="d-flex flex-row-reverse gap-1">
              <button type="button" class="btn btn-outline-secondary" @click="newVisitorFormVisible = false">Отмена</button>
              <button type="button" class="btn btn-outline-secondary" @click="addVisitor">Добавить</button>
            </div>
          </div>
        </div>
      </div>

      <div class="input-group mb-2">
        <span class="input-group-text">Тема</span>
        <input type="text" class="form-control" v-model="newConsultation.topic">
      </div>

      <div class="form-check mb-2">
        <label class="form-check-label">
          <input class="form-check-input" type="checkbox" v-model="newConsultation.primary">
          Первичное обследование
        </label>
      </div>

      <div class="form-check mb-3">
        <label class="form-check-label">
          <input class="form-check-input" type="checkbox" v-model="newConsultation.visitorArrived">
          Клиент прибыл
        </label>
      </div>

      <div class="input-group mb-2">
        <label class="input-group-text">Тип</label>
        <select class="form-select" v-model="newConsultation.type">
          <option v-for="type in consultationTypes" :value="type.name">{{ type.title }}</option>
        </select>
      </div>

      <template v-if="newConsultation.type === 'individualConsultation'">
        <div class="input-group mb-2" v-if="isPsychologist">
          <label class="input-group-text">Код обращения</label>
          <select class="form-select" v-model="newConsultation.requestCode">
            <option v-for="code in requestCodes" :value="code.name">{{ code.name }} - {{ code.title }}</option>
          </select>
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Заметки</label>
          <textarea class="form-control" v-model="newConsultation.notes"></textarea>
        </div>
      </template>
      <template v-else-if="newConsultation.type === 'individualWork'">
        <div class="input-group mb-2">
          <span class="input-group-text">Цель</span>
          <input type="text" class="form-control" v-model="newConsultation.purpose">
        </div>
      </template>
      <template v-else-if="newConsultation.type === 'diagnosticWork'">
        <div class="input-group mb-2">
          <label class="input-group-text">Код обращения</label>
          <select class="form-select" v-model="newConsultation.requestCode">
            <option v-for="code in requestCodes" :value="code.name">{{ code.name }} - {{ code.title }}</option>
          </select>
        </div>
        <div class="input-group mb-2">
          <span class="input-group-text">Тема обращения</span>
          <input type="text" class="form-control" v-model="newConsultation.subject">
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Выявлено в ходе диагностики</label>
          <textarea class="form-control" v-model="newConsultation.revealed"></textarea>
        </div>
        <div class="mb-2">
          <label class="form-label mb-0">Назначено по результатам диагностики</label>
          <textarea class="form-control" v-model="newConsultation.prescribed"></textarea>
        </div>
      </template>

      <button class="w-100 btn btn-outline-secondary" @click="addConsultation">Добавить</button>

    </div>
  </div>
</template>

<script setup>
import { useStore } from "vuex";
import { computed, onMounted, reactive, ref, watch } from "vue";
import { DateTime } from "luxon";
import Multiselect from 'vue-multiselect'
import "vue-multiselect/dist/vue-multiselect.css"
import { callDelete, callGet, callPost, callPut } from "@/services/api.js";
import { parseVisitor, parseConsultation, parseSpecialist } from "@/store/modules/common.js";
import { RequestError } from "@/exceptions.js";
import iziToast from "izitoast";
import {
  visitorTypes, consultationTypeEndpoints, errorToText,
  getFullName,
  getVisitorLabel,
  getVisitorTypeTitleByName, getConsultationTypeEndpoint,
  getConsultationTypeTitleByName, requestCodes, checkSpecialistIsPsychologist, getConsultationTypesForSpecialist
} from "@/utils/commonUtils.js"
import router from "@/router/index.js";

const store = useStore();

const props = defineProps({
  specialistId: Number,
  datetime: DateTime
})

const localSpecialist = ref(null);
const specialist = computed(() => localSpecialist.value ?? store.state.common.specialists?.find(s => s.id === props.specialistId));
const isPsychologist = computed(() => checkSpecialistIsPsychologist(specialist.value?.type ?? ''));

const consultationTypes = computed(() => getConsultationTypesForSpecialist(specialist.value?.type))

const visitors = computed(() => store.state.common.visitors ?? []);
const selectedVisitor = ref(null);
const newVisitorFormVisible = ref(false);
const newVisitor = reactive({ firstName: '', lastName: '', patronymic: '', birthday: '', type: visitorTypes[0].name });

const isLoaded = ref(false);
const consultation = ref(null);
const newConsultation = reactive({
  scheduleDate: null, time: null,
  visitorId: '', topic: '', visitorArrived: false,
  type: consultationTypes.value[0].name, primary: true,
});
const getVisitorOptionSearchText = c => `${getFullName(c)} ${getVisitorTypeTitleByName(c.type)} ${c.birthday.toFormat('dd.MM.yyyy')}`;

onMounted(async () => {
  try {
    let rawConsultation = await callGet(`/api/consultations/${props.specialistId}/${props.datetime.toFormat("yyyy-MM-dd/HH:mm")}`);
    consultation.value = parseConsultation(rawConsultation);

    if (!specialist.value) {
      let rawSpecialist = await callGet('/api/specialists/' + props.specialistId);
      localSpecialist.value = parseSpecialist(rawSpecialist);
      console.warn(isPsychologist.value)
    }
  } catch (err) {
    if (!(err instanceof RequestError && err.status === 404)) throw err;
  }
  isLoaded.value = true;

  if (!store.getters.visitorsLoaded)
    await store.dispatch('loadVisitors');
});

async function addVisitor() {
  let visitor = await callPost(`/api/visitors`, newVisitor);
  store.commit('addVisitor', visitor);
  selectedVisitor.value = visitors.value.find(c => c.id === visitor.id);
  newVisitorFormVisible.value = false;
  newConsultation.visitorId = selectedVisitor.value?.id;
}

async function addConsultation() {
  const c = newConsultation;
  let data = {
    specialistId: props.specialistId,
    scheduleDate: props.datetime.toFormat("yyyy-MM-dd"),
    time: props.datetime.toFormat("HH:mm"),
    visitorId: selectedVisitor.value?.id,
    topic: c.topic,
    visitorArrived: c.visitorArrived,
    primary: c.primary,
  };

  if (c.type === 'individualConsultation')
    data = { ...data, requestCode: isPsychologist.value ? c.requestCode : '-', notes: c.notes };
  else if (c.type === 'individualWork')
    data = { ...data, purpose: c.purpose };
  else if (c.type === 'diagnosticWork')
    data = {
      ...data,
      requestCode: c.requestCode, subject: c.subject,
      revealed: c.revealed, prescribed: c.prescribed
    };
  const url = `/api/consultations/${getConsultationTypeEndpoint(c.type)}`;

  let createdConsultation = await callPost(url, data);
  consultation.value = parseConsultation(createdConsultation);
}

async function updateConsultation() {
  const c = consultation.value
  let data = { visitorId: c.visitor.id, topic: c.topic, primary: c.primary, visitorArrived: c.visitorArrived };
  if (c.type === 'individualConsultation')
    data = { ...data, requestCode: c.requestCode, notes: c.notes };
  else if (c.type === 'individualWork')
    data = { ...data, purpose: c.purpose };
  else if (c.type === 'diagnosticWork')
    data = {
      ...data,
      requestCode: c.requestCode, subject: c.subject,
      revealed: c.revealed, prescribed: c.prescribed
    };
  const url = `/api/consultations/${getConsultationTypeEndpoint(c.type)}/${consultation.value.id}`;
  const updatedConsultation = await callPut(url, data);
  consultation.value = parseConsultation(updatedConsultation);

  iziToast.info({ title: 'Данные обновлены.', layout: 2, timeout: 2000 });
}

async function deleteConsultation() {
  await callDelete(`/api/consultations/${consultation.value.id}`);
  router.push({ name: 'scheduleDay', params: { specialistId: props.specialistId, date: props.datetime.toISODate() } });
}


// TODO: https://vue-multiselect.js.org/#sub-asynchronous-select

</script>

<style lang="css">

.multiselect__placeholder {
  color: #adadad;
  display: inline-block;
  margin-bottom: 8px;
  padding: 0 0 0 5px;
  font-size: 16px;
  line-height: 20px;
}

</style>
