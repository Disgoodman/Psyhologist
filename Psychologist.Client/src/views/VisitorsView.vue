<template>
  <div class="container-lg mt-3">

    <div v-if="!visitorsLoaded" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="visitors.length === 0" class="alert alert-warning d-flex justify-content-center" role="alert">
      Посетителей нет
    </div>

    <div v-if="newVisitorFormVisible === false" class="mb-3 d-flex">
      <button type="button" class="btn btn-outline-secondary flex-grow-1"
              @click="newVisitorFormVisible = true">Добавить посетителя
      </button>
    </div>

    <div v-if="newVisitorFormVisible" class="card mb-3">
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
          <span class="input-group-text">Отвество</span>
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

        <div class="d-flex flex-row-reverse">
          <button type="button" class="btn btn-outline-secondary" @click="newVisitorFormVisible = false">
            Отмена
          </button>
          <button type="button" class="btn btn-outline-secondary me-1" @click="addVisitor">Добавить</button>
        </div>
      </div>
    </div>

    <!-- TODO: search -->
    <form class="mb-3 d-flex" @submit.prevent="search">
      <input v-model="searchText" class="flex-grow-1 form-control" placeholder="ФИО / Дата рождения / Тип">
      <button type="submit" class="ms-1 btn btn-outline-secondary">Поиск</button>
    </form>

    <table class="table table-hover visitors-table">
      <thead>
      <tr>
        <th scope="col">ФИО</th>
        <th scope="col">Дата рождения</th>
        <th scope="col">Тип</th>
        <th scope="col">Действия</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="visitor in filteredVisitors" @click="openVisitorPage(visitor)">
        <th scope="row">{{ getVisitorFullname(visitor) }}</th>
        <td>{{ visitor.birthday.toFormat('dd.MM.yyyy') }}</td>
        <td>{{ getVisitorTypeTitleByName(visitor.type) }}</td>
        <td>
          <div class="d-flex">
            <button type="button" class="btn btn-outline-secondary btn-sm"
                    @click.stop="deleteVisitor(visitor)">
              Удалить
            </button>
            <!--<button type="button"
                    class="btn btn-outline-secondary btn-sm me-1"
                    :disabled="editedVisitor?.id === visitor.id"
                    @click="editVisitor(visitor)">
              Редактировать
            </button>-->
          </div>
        </td>
      </tr>
      </tbody>
    </table>

  </div>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { useStore } from "vuex";
import { computed, onMounted } from "vue";
import { callDelete, callPost, callPut } from "@/services/api.js";
import { DateTime } from "luxon";
import { useRouter } from "vue-router";
import { getVisitorFullname, getVisitorLabel } from "@/utils/commonUtils.js";

const store = useStore();
const router = useRouter();

const visitors = computed(() => store.state.common.visitors);
const filteredVisitors = ref([])
const visitorsLoaded = computed(() => store.getters.visitorsLoaded);

watch(visitors, v => filteredVisitors.value = v, { immediate: true })

const visitorTypes = [
  { name: 'student', title: 'Студент' },
  { name: 'parent', title: 'Родитель' },
  { name: 'specialist', title: 'Специалист' },
]
const getVisitorTypeTitleByName = name => visitorTypes.find(t => t.name === name)?.title;

const newVisitor = reactive({ firstName: '', lastName: '', patronymic: '', birthday: '', type: visitorTypes[0].name });
const newVisitorFormVisible = ref(false);
const searchText = ref('');

onMounted(async () => {
  await store.dispatch('loadVisitors');
});

async function addVisitor() {
  let visitor = await callPost(`/api/visitors`, newVisitor);
  store.commit('addVisitor', visitor);
  newVisitor.title = newVisitor.text = '';
  newVisitorFormVisible.value = false;
}

async function deleteVisitor(visitor) {
  await callDelete(`/api/visitors/${visitor.id}`);
  store.commit('deleteVisitor', visitor.id);
}

function openVisitorPage(visitor) {
  router.push({ name: 'visitor', params: { id: visitor.id } });
}

function search() {
  filteredVisitors.value = visitors.value.filter(c =>
      getVisitorLabel(c).toLowerCase().includes(searchText.value.toLowerCase()));
}


</script>

<style scoped>
h5 {
  white-space: pre-wrap;
}

.visitors-table > tbody > tr {
  cursor: pointer;
}
</style>