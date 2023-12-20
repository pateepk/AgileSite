const path = require('path')
const merge = require('webpack-merge')
const baseWebpackConfig = require('./webpack.base.config')
const HardSourceWebpackPlugin = require('hard-source-webpack-plugin');

const devWebpackConfig = merge(baseWebpackConfig, {
  output: {
    path: path.resolve('./', './dist/'),
    publicPath: 'http://localhost:3000/dist/'
  },
  mode: 'development',
  serve: {
    port: 3000,
    hotClient: true,
    clipboard: false,
    devMiddleware: {
      publicPath: 'http://localhost:3000/dist/',
      headers: {
        "Access-Control-Allow-Origin": "*",
      },
    },
  },
  // https://webpack.js.org/configuration/devtool/#development
  devtool: 'cheap-module-eval-source-map',
  // Use compilation caching
  plugins: [ new HardSourceWebpackPlugin()],
});

module.exports = devWebpackConfig;
