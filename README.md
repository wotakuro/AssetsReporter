# AssetsReporter

Read this in other languages: English, [日本語](README.ja.md)<br />

[Unity] You can detect strange import settings on web browser.<br />
<br />
Purpose of this： This software was developed for detect strange "Import settings"(for texture/audio/model).<br />
Copy this to your project.
(Not only Assets directory, but also AssetsReport directory.)

<pre>For Japanese (日本の方向け)
branchをjapaneseにする事で日本語版になります。
</pre>

<pre>To customize this.
If you wanted to customize this, you'd better to watch wiki.
I wrote a document "how this system works".
</pre>

# Demo
[`https://wotakuro.github.io/AssetsReporter/`](https://wotakuro.github.io/AssetsReporter/)<br />
You can watch result of AssetsReport.


# How to use
![alt text](doc/image/menu.png)
<pre>
Click "Tools/AssetsReporter" to open window.
</pre>

![alt text](doc/image/reporterWindow.png)
<pre>
This reports independentae of each part( "Texture/Model/Audio").
Push "Report" button to analyze project, then open web browser.
</pre>


# TextureReporter
![alt text](doc/image/textureReporter.png)
<pre>
This reports Import settings of Textures.
You can use this like these cases.
 - Is compression format suitable?
 - Are there any textures which size isn't power of two.
</pre>

# ModelReporter
![alt text](doc/image/modelReporter.png)
<pre>
This reports Import settings of Models.
You can use this like these cases.
 - Is optimize mesh ?
 - Is Rig suitable?
 - Are there any models which is checked "Read/Write".
</pre>

# AudioReporter
![alt text](doc/image/audioReporter.png)
<pre>
This reports Import settings of AudioClips.
You can use this like these cases.
 - Is compress format suitable?
</pre>

# AssetBundleReporter
![alt text](doc/image/ReporterAb.png)
<pre>
You'll be able to watch assetbundle data on web browser.
</pre>

# ResourcesReporter
![alt text](doc/image/ResourcesReporter.png)
<pre>
It waste runtime-memory to put "Resources" directory in your project.
You should better to replace "Resources" directory to assetbundle.
This helps you to find "Resources" directory and assets in the directory.
</pre>

#about ignore
<pre>
report results are in below.
 "/AssetsReporter/AssetsReporter/result/*.js"
 "/AssetsReporter/AssetsReporter/result/preview/"
you'd better to add ignore list.
</pre>


## TODO
refactoring<br/>

# Other
This software is depends on jquery.
https://jquery.com/
