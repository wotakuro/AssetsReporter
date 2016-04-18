# AssetsReporter
[Unity] Import設定などをブラウザ上で閲覧/検索できるようにするツールです<br />
<br />
目的：不適切なImport設定になっているテクスチャ/サウンド/モデルデータ等を見つけるためのツールです。<br />
このツールはUnityプロジェクト内(Assets以下)に含むEditor拡張と、プロジェクト内に含めないHTML部分の二個で成り立っています<br />
本プロジェクトを丸ごとコピーして利用して下さい。



# 使い方
![alt text](doc/image/reporterWindow.png)
<pre>
メニューの「Tools/AssetsReporter」を押すことで上記Windowが開きます。

Texture/Model/Audio等の項目別に呼び出して使います。
「Report」ボタンを押すとプロジェクト内を解析し、ブラウザで表示することが出来ます。

それぞれについては以下にて。

</pre>

## TODO
リファクタリング<br/>
png以外の画像フォーマットがサイズがおかしくなるので対応（tga等)
HTML上でもファイル名絞り込み機能つける。<br />
ModelReporterはポリゴン数で検索できるようにする

# TextureReporter
![alt text](doc/image/textureReporter.png)
<pre>
プロジェクト内のTextureの設定をレポートします。
圧縮フォーマットは適切かなどの確認等に使えればと思います。
テクスチャサイズは現在 png以外のサイズが取れていないので修正予定。（ファイルから直接取らない方針に切り替え予定）
</pre>

# ModelReporter
![alt text](doc/image/modelReporter.png)
<pre>
プロジェクト内のModelの設定をレポートします。
Rigの設定はどうなっているか、Mesh最適化等はちゃんとしているか等々確認できます
</pre>

# AudioReporter
![alt text](doc/image/audioReporter.png)
<pre>
プロジェクト内のAudioの設定をレポートします。
圧縮フォーマット等の確認にお使いください
</pre>

# AssetBundleReporter
<pre>
作りかけです。作成予定のAssetBundleを閲覧可能な形にします
</pre>

#ignoreについて
<pre>
以下の二つを追加すると良いでしょう…
「/AssetsReporter/AssetsReporter/result/*.js」
「/AssetsReporter/AssetsReporter/result/preview/」
</pre>
