# M6a: prototip yerleşimine entegrasyon

Sahne: `Assets/Game/Scenes/TrainIntegration.unity`. Orijinal `SampleScene 2` ve önceki küçük laboratuvarlar korunur. Bu sahne mevcut savaş/onarım/koşu çekirdeğini hedef geometriyle birleştirir; nihai sanat, UI, denge veya bot uygulaması değildir.

**Güncel D23 revizyonu:** kullanıcı yeni görsellerle eski prefab ölçülerinin düzeltilmesini istedi. Sahne artık veriyle kurulan 12 × 4,2 blok vagon, 1,7 boyunda kapsüller, 4/5/6/5/4 kapı dizilimi ve responsive perspektif kullanır. Güncel kurulum/ölçü/model bağlama [VISUAL_SCALE.md](VISUAL_SCALE.md) içindedir. Aşağıdaki ilk referans ölçüleri ve test zamanları M6a'nın önceki sürümünün tarihsel kaydıdır.

## İlk M6a referansı ve kurulum geçmişi

`TrainIntegrationBuilder`, referans sahnedeki ilk `Vagons` çocuğunu (`Assets/Scripts/Door/yenivagonlar/PF_Wagon_01a_301.prefab`) görsel olarak kopyalar. Eski gameplay scriptleri/colliderları entegrasyon kopyasından çıkarılır; kaynak prefab değişmez. Sahne ölçeğinde gövde yaklaşık 6,39 × 2,71; kapı genişliği yaklaşık 0,92; üç giriş iki yanda karşılıklıdır. 7,41 birim vagon aralığı referans yerleşime dayanır. Kapı X konumları/genişlikleri gerçek `DOORS` renderer bounds verisinden alınır.

Kamera referansın perspektif FOV 40 / 70° eğimini kullanır. `RunPresentation` tek takip sahibidir; kamera X'i oyuncuyu izler, Y/Z kadrajı sabittir. Eski `CameraFollow`/Cinemachine sürücüsü yeni kameraya eklenmez. Yeşil kapsül, beyaz zombi ve kırmızı/cam panel görselleri geçici test sunumudur; vagon gövdesi gerçek prototip modelidir. Kadraj ve oynama hissi kullanıcı değerlendirmesine açıktır.

Unity menüsü: **Wait Your Turn → Integration → Build One/Two/Five Wagons**. Bunlar aynı entegrasyon sahnesini seçilen vagon sayısıyla yeniden kurar; elde yapılan entegrasyon sahnesi değişikliklerini koruyan bir güncelleme aracı değildir. Eski lab sahneleri ve referans değişmez. Doğru colliderlardan yeni NavMesh bake edilir; eski lab verisi ölçeklenmez. `PrototypeGeometryProbe` referans ölçülerini `Logs/PrototypeGeometry.txt` içine çıkarır.

## Sahiplik ve bağımlılıklar

- `WagonGeometry` (Train): yerel gerçek iç hacim, kapı adayları, istasyon doğuş çapaları ve güvenli iç konumlar. Player/Enemies/Run bağımlılığı yoktur.
- `EnemyPool`: yalnız kendi vagonunun geometrisinden erişilebilir giriş seçer. Yol sorgusu spawn sırasında yeniden kullanılan `NavMeshPath` ile yapılır; her kare bütün kapılarda path araması yoktur. Aynı tarafta tam rotası olan en yakın yaklaşma noktası seçilir. Sonraki taktik profillerinin açık kapıyı tercih etmesi M6 işidir.
- `EnemyBrain`: gerçek iç hacim üyeliği ile portal eşiğini ayırır. Karşı platform/komşu vagon içerisi sayılmaz. Bekleme ofseti portalın yerel eksenindedir. İç takip hedefi vagon sınırlarına alınır. İçeri geçen düşman oyuncuya saldırmadan önce eşiği temizler.
- `WagonRuntime`: doğuş/güvenli konumlar sahnedeki çapaları kullanır; en yakın onarım kapısı kendi kapı listesinden seçilir (D23: 4–6). Kalkışta içerideki/eşikte içeri girmiş bedenler tutulur; dışarıdakiler ödülsüz havuza döner. İşgal edilen eşik kapanmadan önce güvenli iç aday kontrolü yapılır.
- `MovementArea`: oyuncu sınırı ayrı, kaldırılabilir bir hareket kuralıdır. Kırık kapı bunu kaldırmaz. Yeni geometri düzenlenirken MovementArea ve iç hacim birlikte kontrol edilir.
- Eski tek kapılı laboratuvarlar geriye uyum için eski tek-portal bağlamını kullanır. Yeni sahnede tüm vagonlarda `WagonGeometry` zorunlu olarak bağlanır; eski sabit koordinatlar yeni sahnenin doğuş kaynağı değildir.

İstasyonun iki platformu kesintisizdir. Ortak sabit tren/istasyon colliderlarından tek NavMesh oluşturulur; vagon sahipliği yapay platform kopukluğuyla sağlanmaz. Ayrı **Train Zombie** agent türü radius 0,30 / height 1,70 / climb 0,20 kullanır; eski Humanoid ayarı değiştirilmez. Kapı carving'i yalnız kendi eşik alanını kapatır. `IntegrationNavigation/Train-N.asset` seçilen geometriye ait editör bake çıktısıdır; çalışma anında bake yoktur.

Görsel ray/travers, istasyon platformları ve yol çevresi collider içermez; `RunPresentation` tarafından hareket ettirilir. Colliderlar, spawn çapaları, iç hacim ve baked NavMesh hareket etmez. Savunmada platform görünümü sabit fizik zeminiyle hizalanır; istasyon değişimi karanlıkta yapılır. Dekor colliderı gerekiyorsa sabit gameplay karşılığı ayrıca tasarlanır.

## Doğrulama

D23 güncel ölçüde: 38 can/onarım/geometri/yerleşim + 15 yolculuk/kamera testi PASS. Beş-vagon F10 kabulü 2026-10-08 07:13:38 UTC PASS: 4/5/6 kapı varyantları, kapatılan orta duvarlar, 10 geçiş ve 60 düşman havuz sınırı. 16:9 ve 4:3 Unity kadrajları kontrol edildi. Ayrıntılar ve cihaz doğrulama sınırı [VISUAL_SCALE.md](VISUAL_SCALE.md). Aşağıdaki altı-kapılı sonuçlar önceki sürümün kaydıdır.

Play'de **F10** veya **Check integration** düğmesi temsilî kabulü çalıştırır; sonuç `Logs/TrainIntegration-N.txt` dosyasına yazılır. Test geçici koşuyu sıfırlar; normal oynanışa temiz başlangıçla döner.

Kontroller: altı kapıda kısmi onarım/kimlik korunması, cam görüşü, duvar ve alt panel engeli; sağlam/kırık kapıda oyuncu sınırı; altı yönden eşzamanlı yaklaşma/kapıya hasar/giriş; iki taraftaki işgal edilmiş eşiğin güvenli kapanışı; içerideki/dışarıdaki ayrımı; oyuncu X kamera takibi; 10 istasyon geçişinde bütün vagonların ziyaret edilmesi; oyuncu canı, seçili SMG/mermi, her kapının hasarı ve içerideki düşmanların korunması. Son olarak havuz kapasitesi doldurulur, fazladan üretim reddedilir ve vagon/nav bağlamı kontrol edilir.

EditMode: mevcut can/onarım grubuna iki döndürülmüş/ölçeklenmiş vagon sınırı vakası eklenmiştir; 34/34 geçti (2026-10-08 05:57:10 UTC). İlk tek-vagon kontrolü 05:58:28 UTC, iki-vagon kontrolü 06:01:01 UTC geçti. Bu ilk kontrollerden sonra ortak kesintisiz platform topolojisine geçildi; güncel çoklu vagon sonucu aşağıda ayrıca kaydedilir.

Güncel kesintisiz platformla iki vagon kabulü 06:05:32 UTC, beş vagon kabulü 06:08:40 UTC geçti. İkisinde de 10 atama, tüm vagonların ziyaret edilmesi, altı kapı kuralları ve durum kalıcılığı doğrulandı. Son kapasite kontrolünde sırasıyla 24 ve 60 eşzamanlı düşman nav/vagon bağlamını korudu; 13. düşman her vagon havuzunda reddedildi. Yolculuk/evre regresyonu 06:12:03 UTC: 11/11 PASS. Son Play ve EditMode kontrollerinde Console yeni hata/uyarı üretmedi.

Henüz doğrulanmayanlar: bu yeni birleşimin Android performansı/uzun oturumu, farklı zombi türleri, gerçek botlar, nihai UI/kadraj/animasyon hissi. Kısa PC kabulü Android FPS kanıtı değildir.

## Sonraki iş: M6

Mevcut `StationSpawner` küçük, sınırlı M4 adaptörüdür; programlı tür dağılımı veya global üretim kuyruğu değildir. Bir sonraki uygulama `StationDefinition` verisi, ortak enemy profilleri, global/vagon canlı sınırı ve kare başına üretim bütçesidir. Bu sahnenin `WagonGeometry` doğuş/kapı verisini kullanır; tek kapılı lab koordinatlarına dönülmez. İçeride taşınan düşmanlar canlı bütçesine dahil edilir. Açık kapıya yeniden hedef seçimi ayrı taktik davranışı olarak eklenir; mevcut en yakın erişilebilir giriş seçimiyle karıştırılmaz.
