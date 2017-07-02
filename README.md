# WeatherAPI
天気予報のデータが必要な機会は増えていますが、意外に適当な入手方法がありません。

気象庁の防災情報XMLは無料で利用も自由なのですが、複雑で誰でも使えるようなものではありません。
それだったら、もう少し使い易いものにしていこうと思って始めました。

### Livedoor の「お天気Webサービス」互換のAPIのベータ版公開しています
お天気Webサービス（Livedoor Weather Web Service / LWWS）互換のAPIで、全国142カ所の天気予報と府県天気概況を提供しています。
お天気Webサービスのプロパティのうち、pinpointLocation 及び copyright 以外のものを提供しています。
プロパティに関しては、気象庁の天気予報、週間天気予報のホームページにある内容であれば追加できますので Issues からリクエストしてください。

JSONデータをリクエストする際のベースとなる基本URLは以下になります。  
http://weather2.time-j.net/wws/v1

このURLに city のパラメータに地域IDを設定したものを加えてリクエストします。 

例「徳島県・徳島の天気」を取得する場合  
基本URL + 徳島のID（360010）  
http://weather2.time-j.net/wws/v1?city=360010 

地域IDの一覧は、Documents フォルダー内の「天気予報地点定義.xlsx」にあります。

※このサービスも無保証です。

### JmaXmlプログラム（アルファ版）について
気象庁が公開している「気象庁防災情報XMLフォーマット形式電文」を取得して、Jsonデータに変換するプログラムです。まだ、開発の初期段階です。

- JmaXmlServer XML電文（PUSH型）の更新情報を受け取るためのSubscriberです。
- JmaXmlClient 受信したXML電文を元に天気予報等のXMLを取得し、Jsonデータに変換するコンソールアプリケーションです。

C# + .NET Core で作成しています。Linux でも Windows でも動作します。

データベースに Google Cloud Datastore を使用しているので、このプログラムを稼働させるには Google Cloud Platform（GCP）を使うのが便利です。

#### GCPは無料で使えます

Google Cloud Datastore を使うようにした理由は、このプログラムを動かすためにはサーバーが必要ですが、GCPには無料枠があって、この程度のプログラムであれば無料で試せるためです。GCP の Compute Engine には Always Free があって、米国リージョンを使えば f1-micro インスタンスが1インスタンス無料で使えます。米国リージョンの中では日本に近いオレゴン（us-west1）リージョンを使うのがベターです。Google Cloud Datastore にも無料枠があります。 

### 参考

OSSの天気アイコン https://erikflowers.github.io/weather-icons/
