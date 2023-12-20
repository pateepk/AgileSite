<template>
  <!-- Component design is taken from https://brand.kentico.com/d/8mGnybcVIQZM/brand-identity#/basics/colors/complementary-colors -->
  <div class="colors-wrapper">
    <h3>{{ title }}</h3>
    <ul class="colors">
      <li v-for="color in colors" :key="color.name" class="color">
        <div class="color-preview-wrapper">
          <div class="color-preview" :style="{ backgroundColor: color.hex }"></div>
        </div>
        <div class="color-name-wrapper">
          <span class="color-name" :title="color.name">{{ color.name }}</span>
        </div>
        <ul class="colorspaces">
          <li>
            <span class="color-value color-value-hex">
              <span class="color-mode">HEX</span>
              <span data-field="hex" data-prefix="#">{{ color.hex }}</span>
            </span>
          </li>
        </ul>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  props: {
    title: String,
    colors: Array,
  },
};
</script>

<style lang="less">
  .colors-wrapper {
    padding: 10px 0;
    margin: 10px 0;
  }

  .colors {
    padding: 0;
    margin: 20px 0;
  }

  .color:first-child {
    border-top: 1px solid #eee;
  }

  .color {
    border-bottom: 1px solid #eee;
    margin: 0;
    padding: 0;
    text-align: left;
    height: auto!important;
    min-height: 37px;
    display: flex;
    width: 100%;
    -webkit-box-align: center;
    -ms-flex-align: center;
    align-items: center;
  }

  .color-preview-wrapper {
    display: flex;
    min-height: 100%;
    width: 60px;
    -webkit-box-orient: vertical;
    -webkit-box-direction: normal;
    -ms-flex-direction: column;
    flex-direction: column;
    -ms-flex-item-align: stretch;
    align-self: stretch;
  }

  .color-preview {
    border-radius: 0;
    -webkit-box-shadow: none;
    box-shadow: none;
    font-size: 10px;
    margin: 0;
    text-align: center;
    width: 60px;
    -webkit-box-flex: 1;
    -ms-flex: 1;
    flex: 1;
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    -webkit-box-pack: center;
    -ms-flex-pack: center;
    justify-content: center;
  }

  .color-name-wrapper {
    -webkit-box-flex: 0;
    -ms-flex-positive: 0;
    flex-grow: 0;
    line-height: 30px;
    padding: 0 10px;
    width: 170px;
    justify-content: left;
    align-items: center;
    font-size: 13px;
  }

  .color-name {
    color: #565454;
    font-weight: 700;
    line-height: 1.5;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    min-width: 0;
  }

  .colorspaces {
    flex-grow: 1;
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .color-value {
    line-height: 30px;
    float: left;
    font-size: 13px;
    font-weight: 400;
    line-height: 30px;
    max-width: none;
    width: 170px;
  }

  .color-mode {
    display: inline-block;

    border-top-left-radius: 2px;
    border-top-right-radius: 2px;

    background-color: #f8f8f8;
    border-bottom-left-radius: 2px;
    border-bottom-right-radius: 2px;
    color: rgba(0,0,0,.4);
    display: inline-block;
    font-size: 11px;
    line-height: 20px;
    margin: 5px 3px 0 0;
    padding: 0 5px;
    vertical-align: top;
  }
</style>
