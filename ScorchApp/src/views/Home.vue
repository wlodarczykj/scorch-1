<template>
  <div class="home container">
    <div class="home-message">
      <h1>{{ msg }}</h1>
    </div>
    <div class="character-card-container">
      <div class="card" v-for="(char, index) in characters" :key="index">
        <character-card :characterInfo="char"></character-card>
      </div>
    </div>
  </div>
</template>

<script>
import { CharacterCard } from 'components/character'

export default {
  name: 'home',
  data () {
    return {
      msg: 'Welcome to Your DnD App',
    }
  },
  created() {
      this.$store.dispatch('getParty')
  },
  computed: {
    characters () {
      return this.$store.getters.myParty;
    }
  },
  components : {
    CharacterCard
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  .home-message {
    text-align: center;
  }
  .card {
    width: 45%;
    margin: 1%;
  }
  .character-card-container {
    margin: 0 auto;
    width: 70%;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
  }
</style>
