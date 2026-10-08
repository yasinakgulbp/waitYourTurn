# Vagonlar ve istasyon döngüsü — M4a / M4b

2026-10-07–08. Bu sahne ortak koşu mekaniğinin laboratuvarıdır; nihai tren modeli veya tamamlanmış Battle modu değildir. Eski prototip ve GameplaySandbox korunur.

## Açma ve oynama

Unity: `Wait Your Turn → Run → Open Two Wagons`, ardından Play. Sahne: `Assets/Game/Scenes/RunSandbox.unity`. WASD/sol joystick; otomatik nişan, ateş ve reload. Sarı onarım noktasında yaklaşık 3 saniye kalınca hasarlı/kırık kapı onarılır. D20: oyuncu hasarı onarımı kesmez, kapı hasarı ilerlemeyi sıfırlar; ikisi ayrı ayarlanabilir (GAMEPLAY_LAB.md). Camdan dışarı ateş ve oyuncunun vagon sınırı M3 bileşenleriyle sürer.

Başlangıç ayarları: ilk yaklaşma 10 s, savunma 60 s, kalkış uyarısı 3 s, hızlanma 3 s, kararma 1.5 s, karanlık sayaç 10 s, açılma 1.5 s; sonraki yaklaşma 6 s. `RunDriver` Inspector'ındaki süreler koşu ayarıdır. `Restart run` yeni koşu başlatır; sıradan istasyon geçişi hiçbir canı doldurmaz.

İki vagon kimliği `wagon-a` / `wagon-b`; kapılar 10 / 20 canlıdır. Tek oyuncu nesnesi taşınır; hedef/onarım/hareket alanı/kamera yeni vagona bağlanır. Eşit olasılık ve aynı vagona tekrar atanma bu laboratuvarın deneme kuralıdır. Tekrarlanabilir test için seed 12345 kullanılır; yayın rastgeleliği henüz seçilmedi.

## Sahiplik

- `RunFlow`: Unity'den bağımsız evre ve süre otoritesi. Bir uzun kare kalkış/kararma/atama callbacklerini atlayamaz; taşan süreyi sonraki evreye yığmaz. Ölüm veya hata terminaldir.
- `RunDriver`: sahne bağlantıları, aynı oyuncuyu güvenli konuma taşıma, karanlık oynanış kapısı. `WagonRuntime`: kapılar, hareket alanı, kendi enemy pool'u ve kalkış üyeliği. Runtime modülü Sandbox/HUD'a referans vermez.
- `EntryPortal.StationAccess` istasyondan yeni giriş iznidir; kapı canı ve fiziksel açıklığı farklı durumdur. Kalkış, kırık kapıyı iyileştirmez. İçeri girmiş zombi sırf oyuncu başka vagona taşındı diye peşinden gitmez. Şimdilik oyuncusuz vagonda içeride bekler; bot/turret hedefleri sonraki kapsamdır.
- Kalkışta eşik merkezinin iç tarafında olan zombi trende kalır; hâlâ eşikteyse çakışmayan, NavMesh üzerinde bir iç noktaya güvenli tamamlanır. Dış taraftakiler ödülsüz havuza döner. Normal girişte teleport yoktur.
- Atama için sınırlı sayıda iç nokta NavMesh, oyuncu hareket alanı ve fiziksel işgal bakımından doğrulanır. Hiçbiri güvenli değilse vagon canı/zombi silinmez; koşu `Faulted` ile durup tanı gösterir. Daha kalabalık gerçek tren için doğuş politikası ve kısa koruma değerlendirmesi M4b'de açık kalır.
- Karanlıkta ölçekli oyun saati, enemy motorları, girdi/otomatik hedefleme, onarım ve hasar durur. Evre sayacı ve görsel çevre hareketi gerçek zamanla sürer. Önceki dokunulmazlık ve zaman ölçeği geçiş sonunda geri yüklenir. Görünür yolculukta iç zombilerle savaş sürer.
- `StationSpawner`: ilk küçük süre/bütçe adaptörü. İlk istasyonda vagon başına 8 deneme, sonraki istasyonlarda +1, en çok 20; 2 s aralık. Her pool 12 gövde, toplam 24. Taşınan zombiler aynı sınıra dahildir. Dolu/uygunsuz spawn denemesi tüketilir; sınırsız kuyruk veya sonradan ani üretim yoktur. M6 veri temelli spawn yönetiminin yerini almaz.
- `RunPresentation`: yalnızca collider taşımayan çevre görsellerini ve kamerayı hareket ettirir. Gameplay zemini sabittir. İlk sürümde aynı baked NavMesh asset'i paylaşılmıştı; M4b kalıcı kayıt düzeltmesi her vagon için ayrı kopya kullanır. Sinematik tren/istasyon assetleri ve ses henüz bağlı değildir.

## Doğrulama ve sınırlar

Windows Editor'da ilk 10 ardışık hızlandırılmış istasyon geçişi PASS (UTC `2026-10-07T19:24:25.2976378Z`). Gerçek bileşenlerle: aynı oyuncu yaşamı/85 can/7 mermi, kırık A ve 16 canlı B kapısı; içerideki zombinin vagon kimliği, ödülsüz dış temizlik, karanlık saatin/hareketin/onarımın/hasarın durması, 10 atama, toplam 24 pooled gövde ve ölümden sonra GameOver korunması kontrol edildi. Hızlandırılmış testte oyuncu korunur ve yeni spawn kapalıdır; bu bir zorluk/oynanış dengesi kanıtı değildir.

Normal 60 s savunma da Play'de ikinci istasyona ilerledi; kapı hasarı ve oyuncusuz B vagonundaki iç zombiler korundu. Yeni Android buildi yapılmadı; önceki A54 ölçümleri bu iki vagonlu akışın kanıtı değildir.

2026-10-08 son kontrol: kapının iç eşiğinde tutulan ek zombiyle **10/10 geçiş PASS**, UTC `2026-10-08T02:59:15.8325262Z`. Eşikteki zombi güvenli iç noktaya tamamlandı ve sonraki istasyonlarda korundu. Ayrıca **6 EditMode evre testi PASS** (UTC `02:57:54`): uzun karede evre atlamama, yaklaşma/kararma/karanlık/açılmada terminal ölüm ve başarısız kalkıştan sonra atamaya gitmeme. Bunlar hasar kapısını atlayıp karanlıkta öldürme anlamına gelmez; saf evre otoritesinin terminal davranışını sınar.

Kullanıcının bildirdiği kırmızı kayıt gerçekti: Play kapanışında ayrı mermi izi nesnesi önce yok olunca `ShotTracer.OnDisable` silinmiş `LineRenderer`'a erişiyordu. Kapanışta varlık kontrolü eklendi; son Play giriş/test/çıkışında Console **0 hata / 0 uyarı**. Test kodundaki geçici NUnit derleme hatası da giderildi. Normal saldırı yeniden gözlendi: oyuncusuz B vagonunda 7 aktif zombi ve kapı 20 → 0; ilk 10 s yaklaşmada spawn olmaması tasarım gereğidir. Bot rakip henüz yoktur.

Tekrar çalıştırma: HUD `Check 10 fast station transitions`; EditMode `Wait Your Turn → Run → Run Flow Tests`. Ayrıntılı çıktılar Git dışında `Logs/RunSandboxReport.json` ve `Logs/RunFlowTests.xml`.

Para, mağaza, farklı silahlar, turret, dron, botlar ve disk/bulut kaydı henüz uygulanmadı. Bunların korunması bu testte kanıtlanmış değildir. Aynı oyuncu/vagon kimliklerinin korunması sonraki bileşenlerin bağlanacağı temeldir. Kullanıcı M4a PC değerlendirmesini kabul ederek sonraki aşamayı istedi.

## M4b — Beş vagonlu laboratuvar

2026-10-08. `Wait Your Turn → Run → Open Five Wagons`, ardından Play; sahne `Assets/Game/Scenes/TrainSandbox.unity`. İki vagonlu sahne korunur. Aynı RunFlow/RunDriver yeniden kullanılır; aşama kuralları vagon sayısına göre çoğaltılmaz.

`Assets/Game/Content/FiveWagonLayout.asset` başlangıç yerleşim verisidir: `wagon-a`–`wagon-e`, 10 birim aralık, kapı canları 10/20/30/40/50. Bunlar deneme değerleridir. Asset düzenlendikten sonra TrainSandbox açık ve Play kapalıyken `Apply Five Wagon Layout` sahneye uygular. Can/konum canlı koşu sırasında bu asset'e yazılmaz. Her vagon kendi kapısı, hareket alanı, NavMesh yüzeyi ve 12 gövdeli enemy pool'una sahiptir; beş vagon toplam 60 gövdeyle sınırlıdır. Kamera atanan vagonu izler. Eşit olasılık, tekrar seçilme ve lab seed'i 12345 korunur.

Beş vagonda **10/10 istasyon geçişi PASS**, UTC `2026-10-08T03:22:42.1451119Z`: beş vagonun tümü ziyaret edildi; aynı oyuncu yaşamı/85 can/7 mermi, ayrı kapı canları ve içerideki düşman üyelikleri korundu. Kamera ve oyuncu hareket alanı atamayla birlikte değişti; karanlık pause, eşik tamamlama, ödülsüz dış temizlik ve terminal ölüm kontrolleri geçti. Testte yeni spawn/otomatik ateş kapalı ve oyuncu korunur; normal zorluk dengesi kanıtı değildir. İlk fixture kapı kırılmasının aynı karesinde carved eşikte spawn deneyerek başarısız oldu; fixture artık Unity'nin NavMesh güncellemesine iki kare tanır.

**60 eşzamanlı zombi / 6 saniye PC yük kontrolü PASS**, UTC `2026-10-08T03:23:06.5798719Z`: beş kapı da gerçek enemy saldırısıyla hasar aldı; nav/kimlik/aktif sayı sınırı korundu. İlk saniye dışarıda bırakılan 1161 kare örneğinde ortalama **4.31 ms**, p95 **6.37 ms**. Bu Windows Editor'da kısa, yapay yük örneğidir; Android FPS, AI CPU maliyeti, uzun oturum veya vagon içinde 60 zombilik sıkışmasızlık kanıtı değildir. Önceki kalabalık hedef yerleşimi işi M6'da açıktır.

Tekrar: HUD `Check 10 fast station transitions` ve `Check bounded crowd (6 seconds)`. Git dışı raporlar `Logs/TrainSandboxReport.json` / `Logs/TrainSandboxCrowdReport.json`. Editor'ın domain ve sahne reload'u kapalıyken tekrar Play başlangıcında boş akış gözlendi; standart reload açıldı (`ProjectSettings/EditorSettings.asset`). Bu ayar yalnızca geliştirme başlangıcını etkiler; oyundaki istasyon geçişinde sahne reload edilmez.

Yeniden açma kontrolünde AI Navigation 2.0.14 `NavMeshSurface.OnValidate` paylaşılan sahne-surface asset bağlantılarını boşaltabildi. Önceki açık-sahne testi bu kalıcı kayıt sorununu yakalamıyordu. Builder artık iki/beş vagon sahnesindeki her surface'e `Content/RunNavigation` altında ayrı baked veri kopyası bağlar; koşu sırasında bake yoktur. Vagon geometrisi değişirse bu lab kopyaları da yeniden bake edilmeli; eski NavMesh otomatik olarak yeni geometri sayılmaz. Ortak HUD revizyonu iki vagonlu sahnede de 10/10 geçti (UTC `03:26:22.4258242Z`, nav asset ayrılmasından önce).

Kalıcı nav kopyalarıyla temiz domain/sahne başlangıcından son beş-vagon **10/10 PASS** (UTC `03:30:11.2201728Z`); **60-zombi yük kontrolü tekrar PASS** (`03:30:43.7806154Z`, 1036 örnek, ortalama 4.82 ms/p95 7.38 ms). Son Play kontrolü ve çıkışında Console **0 hata / 0 uyarı**. Bu tekrar, nav kayıt düzeltmesini doğrulamak içindi; saf evre/can kuralları değişmediği için önceki EditMode testleri tekrar koşturulmadı.

**Kalan entegrasyon:** bu genişletme mevcut tek kapılı lab geometrisini çoğaltır. Gerçek tren modeli, vagon başına altı kapı, geometriye ait açık spawn/güvenli iç nokta verileri ve çok kapı hedef seçimi henüz yoktur; bunlar tamamlanmış sayılmaz. Çok kapılı üretim entegrasyonu M6 spawn/rota bütçesinin kabulünden önce ayrıca doğrulanacak. Botlar, gerçek istasyon görseli ve ses bu aşamada eklenmedi. M4b teknik değerlendirmesi kullanıcı tarafından kabul edildi; sonraki uygun Android birleşik kontrolü açıktır.

M5 devam kaydı: dört silah aynı TrainSandbox'a bağlandı. Sınırlı mermili SMG seçimi/23 şarjör/72 yedekle 10/10 geçiş tekrar geçti (UTC `2026-10-08T03:53:08.4632224Z`); önceki HP/kapı/zombi/karanlık kontrolleri de korunur. Farklı silahlar artık uygulanmıştır; M4a sırasında henüz bulunmayan özellikler listesinin bu kısmı güncellendi. Mağaza/para/turret/dron/kayıt hâlâ açık. Ayrıntı WEAPONS_LAB.md.

## Test raylarının hizalanması — 2026-10-08

Kullanıcının görsel geri bildirimiyle iki/beş vagonlu sahnelerin eski yan çubukları kaldırıldı. Traversler vagon zeminlerinin altında, iki ray vagon dizilimine paralel X ekseninde yer alır. `RunPresentation` çevreyi bu doğrultunun tersine kaydırır; iki birimlik tekrar travers aralığıyla eşleşir. Yaklaşmada yavaşlama ve istasyonda duruş Play'de gözlendi. Kamera başka vagona geçtiğinde de aynı kesintisiz ray dizisi görülür. Bu hâlâ basit lab geometrisidir; gerçek tren/istasyon modellerinin yerini almaz.

Builder mevcut iki/beş vagon sahnelerini `Wait Your Turn → Run → Align Lab Track Visuals` ile günceller; yeniden oluşturma ve yerleşim uygulama da aynı görsel kurulumunu kullanır. Ray/travers kökünde yalnızca Transform, MeshFilter ve MeshRenderer vardır; collider veya NavMesh kaynağı eklenmedi. Kayıtlı sahne blokları karşılaştırıldığında görsel kök dışındaki bütün bileşenler aynı kaldı. Silah değerleri değiştirilmedi.

Beş vagonlu mevcut geçiş kontrolü **10/10 PASS**, UTC `2026-10-08T04:29:49.7764496Z`; tüm vagonlar ziyaret edildi ve önceki oynanış koruma kontrolleri geçti. Son Play çıkışında Console **0 hata / 0 uyarı**. Bu küçük görsel düzeltme için Android yük testi tekrarlanmadı.

## Hareketli istasyon ve yol çevresi — 2026-10-08

Kullanıcı yalnız traverslerin hareket etmesinin yolculuk hissini tamamlamadığını belirtti. Önceki sürümde mavi platform, sabit istasyon colliderının Renderer'ıyla çiziliyordu; çevre ayrımı bu kısmı kapsamıyordu. Artık iki/beş vagon sahnelerinde bu Renderer kapalıdır. Aynı ölçü/konumdaki ayrı platform görselleri, sarı platform kenarları ve dekoratif istasyon sütunları `Station Visuals (no physics)` kökündedir. Yol zemini ve referans işaretleri ayrı `Journey Scenery (no physics)` kökündedir. Vagon zemini, kapılar, colliderlar, baked navigation ve spawn/onarım çapaları hareket etmez.

`RunPresentation` aynı evreden üç hareket üretir: ray/travers 2 birim, yol çevresi 20 birim tekrar eder; istasyon **tekrar etmez**. `JourneyMotion` yumuşak hız eğrisinin integralinden istasyon ofsetini hesaplar. İlk yaklaşmanın başında +25 birim (5 hız × 10 saniye / 2), duruşta tam 0; kalkışta geriye doğru uzaklaşma vardır. FadeOut eski istasyondan ayrılmayı sürdürür; tam karanlıkta eski görünüm kapanır. FadeIn yeni istasyonu sonraki yaklaşma süresine uygun mesafeye koyar; açılma → yaklaşma sınırında konum/hız süreklidir. Terminal evre görsel hareketi durdurur; Restart yeni yolculuğu başlatır. Kamera vagon takibini korur. Dekoratif sütunlar fiziksel engel değildir.

Kurulum: Play kapalıyken **Wait Your Turn → Run → Upgrade Journey Visuals** (`Ctrl+Shift+F7`), iki labı günceller; normal açma/yerleşim uygulama da aynı kurulumdan geçer. Görsel kök referansları ve hız `RunPresentation` Inspector'ındadır; runtime kodu sahnedeki nesneleri isimle aramaz. 20 birimlik dekor tekrarına uygun model/doku kullanılmalı; ayarı rastgele değiştirip desen aralığı aynı bırakılmamalı. Gerçek tren modelleri geldiğinde sunum kökleri giydirilir; oynanış geometri verileri ayrıca hazırlanır.

**11/11 EditMode PASS**, UTC `2026-10-08 05:13:24Z`: mevcut 6 evre vakası ve 5 hareket vakası; duruş hizası, hızlanma/kararma sınırı, iki farklı sonraki yaklaşma süresi, kare örneklemesinden bağımsız ofset ve terminal hız. Menü **Run Journey Tests** (`Ctrl+Shift+F8`), rapor `Logs/JourneyTests.xml`.

**10/10 TrainSandbox geçiş PASS**, UTC `2026-10-08T05:14:46.1341671Z`: önceki can/silah/kapı/karanlık/üyelik/havuz kontrollerine ek olarak her duruşta gerçek görsel platform hizası, yaklaşma/kalkışta ofset, sabit gameplay zeminleri ve üç hareketli kökte sıfır collider doğrulandı. Beş vagonun tümü ziyaret edildi, 60 gövde sınırı korundu. Normal 10 saniyelik yaklaşma ve savunmada spawn/kapı saldırısı da gözlendi; Console Play giriş/test/çıkışında **0 runtime hata/uyarı**. İki vagon sahnesi de güncellendi ve dosya yapısı denetlendi; bu revizyonda iki-vagon Play testi ayrıca tekrarlanmadı.

Kaydedilen scene bloklarının karşılaştırmasında mevcut collider, NavMesh, agent, spawn, kapı, silah ve vagon transformları korunur. Beklenen mevcut değişiklikler: platform Renderer'ının kapanması, sunum referansları, önceki D20 varsayılanlarının serileştirilmesi ve yeni görsel kök kayıtları. Yeni bileşen tipleri yalnız GameObject/Transform/MeshFilter/MeshRenderer; runtime instantiate/destroy, yeni fizik sorgusu veya runtime bake eklenmez. Yolculuk her kare sabit sayıda kök taşır; **render maliyeti sıfır sayılmaz**. Bu basit modellerin Android render maliyeti nihai içerik/kalabalık kontrolünde ölçülecek; bu turda cihaz FPS iddiası yoktur.

D21: üretim oyununda zombiler trenin iki tarafından gelebilir. Mevcut tek taraflı/tek kapılı lab bu hedefi tamamlamaz. M6'nın ilk gerçek geometri işi her iki tarafın kapı/yürünebilir alan/spawn verilerini doğrular. Dış zombi üretimi yalnız durmuş istasyonda yapılır; kalkışta dış üyeler havuza döner, içeridekiler korunur. Sunum, spawn/hasar kararının sahibi değildir. Battle ve botsuz mod aynı evre/sunum sözleşmesini kullanabilir; modların kendileri henüz uygulanmadı.
