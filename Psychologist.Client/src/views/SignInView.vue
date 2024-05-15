<template>
	<form class="mx-auto card p-4 w-100 p-2 shadow-sm" @submit.prevent="login">
		<h1 class="h3 mb-3 text-center">Вход</h1>

		<label for="email" class="text-start ms-0">Email</label>
		<input v-model="email" class="form-control mb-3" autocomplete="username" required autofocus>

		<label for="password" class="text-start">Пароль</label>
		<div class="input-group">
			<input type="password" v-model="password"
						 class="form-control rounded" autocomplete="current-password" required>
			<button type="button" class="d-none" v-toggle-password></button>
		</div>

		<button class="btn btn-lg btn-primary w-100 mt-3" type="submit">Войти</button>

	</form>
</template>

<script setup>
import { ref } from "vue";
import store from "@/store";
import router from "@/router";
import vTogglePassword from "@/utils/toggle-password-input";
import { RequestError } from "@/exceptions.js";
import iziToast from "izitoast";

const email = ref('');
const password = ref('');

async function login() {
	try {
		await store.dispatch('login', { email: email.value, password: password.value });
		await router.push("/");
	}
	catch (err) {
		if (err instanceof RequestError && err.status === 401) {
			iziToast.error({
				title: 'Неверный логин или пароль',
				layout: 1,
				timeout: 3000,
				class: "iziToast-api-error"
			});
		} else throw err;

	}
}

</script>

<style scoped>

form {
	max-width: 330px;
	margin-top: 5%;
}

</style>