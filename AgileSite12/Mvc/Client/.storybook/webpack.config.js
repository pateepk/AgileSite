const path = require("path");

module.exports = {
  module: {
    rules: [
      {
        test: /\.less$/,
        loaders: [
          "vue-style-loader",
          "css-loader",
          "less-loader",
          {
            loader: 'style-resources-loader',
            options: {
              patterns: path.resolve(__dirname, 'src/builder/assets/styles/shared.less')
            }
          }
        ],
        include: path.resolve(__dirname, "../")
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
};
