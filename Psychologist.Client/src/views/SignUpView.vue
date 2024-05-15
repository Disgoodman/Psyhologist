<template>
	<form class="mx-auto card p-4 w-100 p-2 shadow-sm" @submit.prevent="register">
		<h1 class="h3 mb-3 text-center">Регистрация</h1>

		<label class="text-start ms-0">Email</label>
		<input type="email" v-model="data.email"
					 class="form-control mb-2" autocomplete="username" required autofocus>

		<label class="text-start">Пароль</label>
		<div class="input-group mb-2">
			<input type="password" v-model="data.password"
						 class="form-control rounded" autocomplete="current-password" required>
			<button type="button" class="d-none" v-toggle-password></button>
		</div>

    <label class="text-start ms-0">Фамилия</label>
    <input type="text" v-model="data.lastName" class="form-control mb-2" required>
    
    <label class="text-start ms-0">Имя</label>
    <input type="text" v-model="data.firstName" class="form-control mb-2" required>
    
    <label class="text-start ms-0">Отчество</label>
    <input type="text" v-model="data.patronymic" class="form-control mb-2">
    
    <label class="text-start ms-0">Дата рождения</label>
    <input type="date" v-model="data.birthday" class="form-control mb-2" required>
    
    <label class="text-start ms-0">Тип</label>
		<select class="form-select" v-model="data.type" required>
			<option v-for="type in visitorTypes" :value="type.name">{{ type.title }}</option>
		</select>

		<button class="btn btn-lg btn-primary w-100 mt-3" type="submit">Зарегистрироваться</button>

	</form>
</template>

<script setup>
import { ref, reactive } from "vue";
import store from "@/store";
import router from "@/router";
import vTogglePassword from "@/utils/toggle-password-input";
import { postRaw } from "@/services/fetch.js";
import { visitorTypes } from "@/utils/commonUtils.js";

const data = reactive({
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  patronymic: '',
  birthday: '',
	type: visitorTypes[0]
})

async function register() {
	await postRaw('/api/register-visitor', data);

	await store.dispatch('login', { email: data.email, password: data.password });

	await router.push({ name: 'home' });
}

</script>

<style scoped>
form {
	max-width: 330px;
	margin-top: 5%;
}
</style>