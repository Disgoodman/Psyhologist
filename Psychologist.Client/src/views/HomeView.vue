<template>
	<div class="container-lg mt-3">

		<div v-if="!articlesLoaded" class="d-flex justify-content-center">
			<div class="spinner-border" role="status">
				<span class="visually-hidden">Loading...</span>
			</div>
		</div>

		<div v-else-if="articles.length === 0" class="alert alert-warning d-flex justify-content-center" role="alert">
			Нет статей
		</div>

		<div v-if="isAdmin && newArticleFormVisible === false" class="mb-3 d-flex">
			<button type="button" class="btn btn-outline-secondary flex-grow-1"
							@click="newArticleFormVisible = true">Добавить статью
			</button>
		</div>

		<div v-if="newArticleFormVisible" class="card mb-3">
			<div class="card-body">
				<input class="form-control mb-3" v-model="newArticle.title" placeholder="Заголовок" />
				<textarea class="form-control mb-3" rows="6" v-model="newArticle.text" placeholder="Текст"></textarea>
				<div class="d-flex flex-row-reverse">
					<button type="button" class="btn btn-outline-secondary" @click="newArticleFormVisible = false">
						Отмена
					</button>
					<button type="button" class="btn btn-outline-secondary me-1" @click="addArticle">Добавить</button>
				</div>
			</div>
		</div>

		<div v-if="editedArticle" class="card mb-3">
			<div class="card-body">
				<input class="form-control mb-3" v-model="editedArticle.title" placeholder="Заголовок" />
				<textarea class="form-control mb-3" rows="6" v-model="editedArticle.text" placeholder="Текст"></textarea>
				<div class="d-flex flex-row-reverse">
					<button type="button" class="btn btn-outline-secondary" @click="cancelEdit">Отмена</button>
					<button type="button" class="btn btn-outline-secondary me-1" @click="updateArticle">Сохранить</button>
				</div>
			</div>
		</div>

		<div v-for="article in articles" :key="article.id" class="card mb-2">
			<div class="card-body">
				<div class="card-title d-flex">
					<h5 class="flex-grow-1 mb-0">{{ article.title }}</h5>
					<p class="ms-3 mb-0">{{ article.time.toFormat("dd.MM.yyyy") }}</p>
				</div>
				<ArticleText class="card-text" :text="article.text" :length="200" />
				<div v-if="isAdmin" class="d-flex flex-row-reverse">
					<button type="button" class="btn btn-outline-secondary btn-sm"
									@click="deleteArticle(article)">
						Удалить
					</button>
					<button type="button"
									class="btn btn-outline-secondary btn-sm me-1"
									:disabled="editedArticle?.id === article.id"
									@click="editArticle(article)">
						Редактировать
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { reactive, ref, unref } from "vue";
import { useStore } from "vuex";
import { computed, onMounted } from "vue";
import ArticleText from "@/components/ArticleText.vue"
import { callDelete, callPatch, callPost, callPut } from "@/services/api.js";

const store = useStore();

const isAuth = computed(() => store.getters.isAuth);
const isAdmin = computed(() => store.getters.isAdmin);

const articles = computed(() => store.state.common.articles);
const articlesLoaded = computed(() => store.getters.articlesLoaded);

const editedArticle = ref(null);
const newArticle = reactive({ title: '', text: '' });
const newArticleFormVisible = ref(false);

onMounted(async () => {
	await store.dispatch('loadArticles');
});

async function addArticle() {
	let article = await callPost(`/api/articles`, newArticle);
	store.commit('addArticle', article);
	newArticle.title = newArticle.text = '';
	newArticleFormVisible.value = false;
}

function editArticle(article) {
	editedArticle.value = { ...article };
}

async function deleteArticle(article) {
	await callDelete(`/api/articles/${ article.id }`);
	store.commit('deleteArticle', article.id);
}

async function updateArticle() {
	let a = editedArticle.value;
	let newArticle = await callPut(`/api/articles/${ a.id }`, { title: a.title, text: a.text });
	store.commit('updateArticle', newArticle);
	editedArticle.value = null;
}

function cancelEdit() {
	editedArticle.value = null;
}


</script>

<style scoped>
h5 {
	white-space: pre-wrap;
}
</style>