# AssetsReporter

Read this in other languages: English, [日本語](README.ja.md)<br />

[Unity] You can detect strange import settings on web browser.<br />
<br />
Purpose of this： This software was developed for detect strange "Import settings"(for texture/audio/model).<br />
Copy this to your project.
(Not only Assets directory, but also AssetsReport directory.)


<pre>To customize this.
If you wanted to customize this, you'd better to watch wiki.
I wrote a document "how this system works".
</pre>

# Demo
[`https://wotakuro.github.io/AssetsReporter/`](https://wotakuro.github.io/AssetsReporter/)<br />
You can watch result of AssetsReport.


# How to use
![alt text](Documentation~/image/menu.png)
<pre>
Click "Tools/UTJ/AssetsReporter" to open window.
</pre>

![alt text](Documentation~/image/reporterWindow.png)
<pre>
This reports independentae of each part( "Texture/Model/Audio").
Push "Report" button to analyze project, then open web browser.
</pre>


# TextureReporter
![alt text](Documentation~/image/textureReporter.png)
<pre>
This reports Import settings of Textures.
You can use this like these cases.
 - Is compression format suitable?
 - Are there any textures which size isn't power of two.
</pre>

# ModelReporter
![alt text](Documentation~/image/modelReporter.png)
<pre>
This reports Import settings of Models.
You can use this like these cases.
 - Is optimize mesh ?
 - Is Rig suitable?
 - Are there any models which is checked "Read/Write".
</pre>

# AudioReporter
![alt text](Documentation~/image/audioReporter.png)
<pre>
This reports Import settings of AudioClips.
You can use this like these cases.
 - Is compress format suitable?
</pre>

# AssetBundleReporter
![alt text](Documentation~/image/ReporterAb.png)
<pre>
You'll be able to watch assetbundle data on web browser.
</pre>

# SceneReporter
![alt text](Documentation~/image/SceneReporter.png)
<pre>
You can see the summary of each scenes.
</pre>

# ResourcesReporter
![alt text](Documentation~/image/ResourcesReporter.png)
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
