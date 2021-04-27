var path = require("path");
var webpack = require("webpack");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');

var CONFIG = {
    indexHtmlTemplate: "./src/Client/index.html",
    fsharpEntry: "./src/Client/Client.fs.js",
    outputDir: "./src/Client/deploy",
    assetsDir: "./src/Client/public",
    devServerPort: 8080,
    devServerProxy: {
        '/api/*': {
            target: 'http://127.0.0.1:' + (process.env.SERVER_PROXY_PORT || "8085"),
               changeOrigin: true
           }
    }
}

var isProduction = !process.argv.find(v => v.indexOf('serve') !== -1);
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var commonPlugins = [
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve(CONFIG.indexHtmlTemplate)
    }),
];

module.exports = [{
    entry: { app: resolve(CONFIG.fsharpEntry) },
    output: {
        path: resolve(CONFIG.outputDir),
        filename: isProduction ? '[name].[chunkhash:8].js' : '[name].js',
		chunkFilename: isProduction ? '[name].[chunkhash:8].js' : '[name].js'
    },
    mode: isProduction ? "production" : "development",
    devtool: false,
    optimization: {
		runtimeChunk: 'single',
		splitChunks: {
			cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        },
    },
    plugins: isProduction ?
        commonPlugins.concat([
            new CopyWebpackPlugin({ patterns: [{ from: resolve(CONFIG.assetsDir) }] }),
        ])
        : commonPlugins.concat([
            new webpack.HotModuleReplacementPlugin(),
        ]),
    devServer: {
        dev: {
			publicPath: "/"
		},
		static: resolve(CONFIG.assetsDir),
        port: CONFIG.devServerPort,
        proxy: CONFIG.devServerProxy,
        hot: !isProduction,
    },
    module: {
        rules: [
            {
                test: /\.(png|jpg|jpeg|gif|svg|woff2|woff|ttf|eot)(\?.*)?$/,
                type: 'asset/resource'
            }
        ]
    }
}];

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}
