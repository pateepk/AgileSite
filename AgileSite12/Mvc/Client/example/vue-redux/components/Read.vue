<template>
  <div class="read">
    <h2>Read only todos</h2>
    <ul class="todos" v-if="todos.items && todos.items.length > 0">
      <li class="todo" :class="{del: todo.done}" v-for="(todo, index) in todos.items" :key="index" v-text="todo.text"></li>
    </ul>
  </div>
</template>

<script>
  import { connect } from 'revux'
  const read = {}

  const mapState = ({
    todos: state => state.todos
  })
  export default connect(mapState)(read)
</script>