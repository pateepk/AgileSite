require('webpack')

const path = require('path')
const stencil = require('@stencil/webpack');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const ExtraneousFileCleanupPlugin = require('webpack-extraneous-file-cleanup-plugin');

module.exports = {
  entry: {
    'page-builder': ['./src/page-builder/main.ts'],
    'form-builder': ['./src/form-builder/main.ts'],
    'page-template-selector': ['./src/builder/assets/styles/page-template-selector.less'],
    'web-components': ['./src/builder/assets/styles/web-components.less']
  },
  output: {
    filename: '[name].js'
  },
  module: {
    rules: [{
      test: /\.vue$/,
      use: [{
        loader: 'vue-loader',
      }]
    },
    {
      test: /\.less$/,
      use: [
        'vue-style-loader',
        'css-loader',
        'less-loader',
        {
          loader: 'style-resources-loader',
          options: {
            patterns: path.resolve(__dirname, 'src/builder/assets/styles/shared.less')
          }
        },
      ]
    },
    {
      test: /\.tsx?$/,
      use: [
        'babel-loader',
        {
          loader: 'ts-loader',
          options: {
            appendTsSuffixTo: [/\.vue$/],
          }
        },
        {
          loader: 'tslint-loader',
          options: {
            configFile: "tslint-loader.json",
          },
        },
      ],
      exclude: /node_modules/
    },
    {
      // https://babeljs.io/docs/plugins/transform-runtime/
      // axios uses Promise which IE11 doesn't support, needs runtime transform
      test: /\.js$/,
      include: [/axios/],
      loader: 'babel-loader'
    },
    {
      test: /\.(png|jpg|gif|svg|eot|ttf|woff)$/,
      loader: 'file-loader',
      options: {
        name: '[name].[ext]?[hash]'
      }
    },
    ]
  },
  resolve: {
    extensions: ['.ts', '.js', '.vue', '.json'],
    alias: {
      'vue$': 'vue/dist/vue.esm.js',
      '@': path.resolve(__dirname, 'src')
    }
  },
  plugins: [
    new VueLoaderPlugin(),
    new stencil.StencilPlugin(),
    new ExtraneousFileCleanupPlugin({
      paths: ['page-template-selector.js', 'web-components.js']
    })
  ]
};
