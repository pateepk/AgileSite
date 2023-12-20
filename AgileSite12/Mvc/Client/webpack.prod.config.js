const path = require('path')
const merge = require('webpack-merge')
const baseWebpackConfig = require('./webpack.base.config')
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const SpeedMeasurePlugin = require('speed-measure-webpack-plugin');
const HardSourceWebpackPlugin = require('hard-source-webpack-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

// Build
let webpackConfig = merge(baseWebpackConfig, {
  output: {
    path: path.resolve('./', './build/')
  },
  performance: {
    hints: false
  },
  optimization: {
    minimizer: [
      new UglifyJsPlugin({
        cache: true,
        parallel: true,
      }),
      new OptimizeCSSAssetsPlugin({}),
    ],
    // Group re-used modules into separated files
    splitChunks: {
      cacheGroups: {
        // Group vendor modules in a single script file
        vendors: {
          test: /[\\/]node_modules[\\/]/,
          name: 'vendors',
          chunks: 'all',
          priority: -10,
        },
        // Group base builder scripts in a single script file
        commons: {
          name: 'builder',
          chunks: 'all',
          minChunks: 2,
          priority: -20,
        }
      }
    }
  },
  plugins: [
    new MiniCssExtractPlugin({
      filename: '[name].css'
    }),
  ]
});

// Run the build command with an extra argument to view the bundle analyzer report after build finishes:
// `npm run build --speedAnalyzer`
if (process.env.npm_config_speedAnalyzer) {
  webpackConfig = new SpeedMeasurePlugin().wrap(webpackConfig);
} else {
  // Use compilation caching only when the speed analyzer is not active
  webpackConfig.plugins.push(new HardSourceWebpackPlugin());
}

// Run the build command with an extra argument to view the bundle analyzer report after build finishes:
// `npm run build --sizeAnalyzer`
if (process.env.npm_config_sizeAnalyzer) {
  webpackConfig.plugins.push(new BundleAnalyzerPlugin());
}

// Use MiniCssExtractPlugin in less loader instead of 'vue-style-loader'
const lessLoader = webpackConfig.module.rules.find(({ test }) => test.test('.less'));
lessLoader.use[0] = MiniCssExtractPlugin.loader;

module.exports = webpackConfig;
