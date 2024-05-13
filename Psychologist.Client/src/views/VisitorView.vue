<template>
  <div class="container-lg mt-3">

    <h3 class="text-center mb-3">Посетитель</h3>

    <div v-if="!visitor" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-if="!editMode">
      <p class="mb-1">ФИО: {{ getVisitorFullname(visitor) ?? '-' }}</p>
      <p class="mb-1">Дата рождения: {{ visitor?.birthday.toFormat('dd.MM.yyyy') ?? '-' }}</p>
      <p class="mb-0">Тип: {{ getVisitorTypeTitleByName(visitor?.type) ?? '-' }}</p>

      <button class="btn btn-outline-secondary mt-2" @click="editMode = true">Редактировать</button>
      <button class="btn btn-outline-secondary mt-2 ms-2" @click="deleteVisitor">Удалить</button>
    </div>

    <div v-if="editMode" class="card mb-3">
      <div class="card-body">
        <div class="input-group mb-3">
          <span class="input-group-text">Имя</span>
          <input type="text" class="form-control" v-model="visitorData.firstName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Фамилия</span>
          <input type="text" class="form-control" v-model="visitorData.lastName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Отвество</span>
          <input type="text" class="form-control" v-model="visitorData.patronymic">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Дата рождения</span>
          <input type="date" class="form-control" v-model="visitorData.birthday">
        </div>
        <div class="input-group mb-3">
          <label class="input-group-text">Тип</label>
          <select class="form-select" v-model="visitorData.type">
            <option v-for="type in visitorTypes" :value="type.name">{{ type.title }}</option>
          </select>
        </div>

        <div class="d-flex flex-row-reverse">
          <button type="button" class="btn btn-outline-secondary" @click="cancelEdit">Отмена</button>
          <button type="button" class="btn btn-outline-secondary me-1" @click="updateVisitor">Сохранить</button>
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
import { reactive, ref, unref, watch } from "vue";
import { useStore } from "vuex";
import { computed, onMounted } from "vue";
import { callDelete, callGet, callPost, callPut } from "@/services/api.js";
import { DateTime } from "luxon";
import { parseVisitor } from "@/store/modules/common.js";
import { useRouter } from "vue-router";
import { visitorTypes, getVisitorFullname, getVisitorTypeTitleByName } from "@/utils/commonUtils.js"

const store = useStore();
const router = useRouter()

const props = defineProps({
  id: Number
});

const visitor = ref(null);

const editMode = ref(false);
const visitorData = ref({ firstName: '', lastName: '', patronymic: '', birthday: '', type: visitorTypes[0].name });
watch(visitor, c => visitorData.value = { ...c, birthday: c.birthday.toISODate() });

onMounted(async () => {
  let rawVisitor = await callGet('/api/visitors/' + props.id);
  visitor.value = parseVisitor(rawVisitor);
  // TODO: return to visitors page if visitor not exists
});

async function deleteVisitor() {
  await callDelete(`/api/visitors/${visitor.value.id}`);
  store.commit('deleteVisitor', visitor.value.id);
  // TODO: return to visitors page or previous page
  await router.push({ name: 'visitors' })
}

async function updateVisitor() {
  let c = visitorData.value;
  let updatedVisitor = await callPut(`/api/visitors/${c.id}`, {
    firstName: c.firstName, lastName: c.lastName, patronymic: c.patronymic, birthday: c.birthday, type: c.type
  });
  visitor.value = parseVisitor(updatedVisitor);
  store.commit('updateVisitor', updatedVisitor);
  editMode.value = false;
}

function cancelEdit() {
  editMode.value = false;
}


</script>

<style scoped>

</style>