<template>
  <div class="container-lg mt-3">

    <h3 class="text-center mb-3">Специалист</h3>

    <div v-if="!specialist" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Загрузка...</span>
      </div>
    </div>

    <div v-if="!editMode">
      <p class="mb-1">ФИО: {{ getFullName(specialist) ?? '-' }}</p>
      <p class="mb-1">Тип: {{ specialist?.type ?? '-' }}</p>
      <p class="mb-1">Цена первичной/вторичной консультации, руб: {{ specialist?.primaryVisitPrice ?? 0 }} /
        {{ specialist?.secondaryVisitPrice ?? 0 }}</p>

      <button class="btn btn-outline-secondary mt-2" @click="editMode = true">Редактировать</button>
      <button class="btn btn-outline-secondary mt-2 ms-2" @click="deleteSpecialist">Удалить</button>
    </div>

    <div v-if="editMode" class="card mb-3">
      <div class="card-body">
        <div class="input-group mb-3">
          <span class="input-group-text">Имя</span>
          <input type="text" class="form-control" v-model="specialistData.firstName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Фамилия</span>
          <input type="text" class="form-control" v-model="specialistData.lastName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Отчество</span>
          <input type="text" class="form-control" v-model="specialistData.patronymic">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Специальность</span>
          <input type="text" class="form-control" v-model="specialistData.type">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Цена первичной/вторичной кносультации, руб</span>
          <input type="number" class="form-control" v-model="specialistData.primaryVisitPrice">
          <input type="number" class="form-control" v-model="specialistData.secondaryVisitPrice">
        </div>

        <div class="d-flex flex-row-reverse">
          <button type="button" class="btn btn-outline-secondary" @click="cancelEdit">Отмена</button>
          <button type="button" class="btn btn-outline-secondary me-1" @click="updateSpecialist">Сохранить</button>
        </div>
      </div>
    </div>

    <!--<h5 class="text-center mt-4 mb-3 fw-normal">Консультации</h5>

    <div v-if="[].length === 0" class="alert alert-secondary d-flex justify-content-center" role="alert">
      Консультаций нет
    </div>-->

  </div>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { useStore } from "vuex";
import { computed, onMounted } from "vue";
import { callDelete, callGet, callPost, callPut } from "@/services/api.js";
import { parseSpecialist } from "@/store/modules/common.js";
import { useRouter } from "vue-router";
import { getFullName } from "@/utils/commonUtils.js"

const store = useStore();
const router = useRouter()

const props = defineProps({
  id: Number
});

const specialist = ref(null);

const editMode = ref(false);
const specialistData = ref({
  firstName: '', lastName: '', patronymic: '', birthday: '', type: '',
  primaryVisitPrice: 0, secondaryVisitPrice: 0
});
watch(specialist, c => specialistData.value = { ...c });

onMounted(async () => {
  let rawSpecialist = await callGet('/api/specialists/' + props.id);
  specialist.value = parseSpecialist(rawSpecialist);
  // TODO: return to specialists page if specialist not exists
});

async function deleteSpecialist() {
  await callDelete(`/api/specialists/${specialist.value.id}`);
  store.commit('deleteSpecialist', specialist.value.id);
  // TODO: return to specialists page or previous page
  await router.push({ name: 'specialists' })
}

async function updateSpecialist() {
  let c = specialistData.value;
  let updatedSpecialist = await callPut(`/api/specialists/${c.id}`, {
    firstName: c.firstName, lastName: c.lastName, patronymic: c.patronymic, type: c.type,
    primaryVisitPrice: c.primaryVisitPrice, secondaryVisitPrice: c.secondaryVisitPrice
  });
  specialist.value = parseSpecialist(updatedSpecialist);
  store.commit('updateSpecialist', updatedSpecialist);
  editMode.value = false;
}

function cancelEdit() {
  editMode.value = false;
}


</script>

<style scoped>

</style>