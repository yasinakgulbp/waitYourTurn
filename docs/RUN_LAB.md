# İki vagon ve istasyon döngüsü — M4a

2026-10-07–08. Bu sahne ortak koşu mekaniğinin laboratuvarıdır; nihai tren modeli veya tamamlanmış Battle modu değildir. Eski prototip ve GameplaySandbox korunur.

## Açma ve oynama

Unity: `Wait Your Turn → Run → Open Two Wagons`, ardından Play. Sahne: `Assets/Game/Scenes/RunSandbox.unity`. WASD/sol joystick; otomatik nişan, ateş ve reload. Sarı onarım noktasında yaklaşık 3 saniye kalınca kırık kapı onarılır. Camdan dışarı ateş ve oyuncunun vagon sınırı M3 bileşenleriyle sürer.

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
- `RunPresentation`: yalnızca collider taşımayan çevre görsellerini ve kamerayı hareket ettirir. Gameplay zemini sabittir; ikinci vagon aynı baked NavMesh verisini kendi yerleşiminde kullanır. Sinematik tren/istasyon assetleri ve ses henüz bağlı değildir.

## Doğrulama ve sınırlar

Windows Editor'da ilk 10 ardışık hızlandırılmış istasyon geçişi PASS (UTC `2026-10-07T19:24:25.2976378Z`). Gerçek bileşenlerle: aynı oyuncu yaşamı/85 can/7 mermi, kırık A ve 16 canlı B kapısı; içerideki zombinin vagon kimliği, ödülsüz dış temizlik, karanlık saatin/hareketin/onarımın/hasarın durması, 10 atama, toplam 24 pooled gövde ve ölümden sonra GameOver korunması kontrol edildi. Hızlandırılmış testte oyuncu korunur ve yeni spawn kapalıdır; bu bir zorluk/oynanış dengesi kanıtı değildir.

Normal 60 s savunma da Play'de ikinci istasyona ilerledi; kapı hasarı ve oyuncusuz B vagonundaki iç zombiler korundu. Yeni Android buildi yapılmadı; önceki A54 ölçümleri bu iki vagonlu akışın kanıtı değildir.

2026-10-08 son kontrol: kapının iç eşiğinde tutulan ek zombiyle **10/10 geçiş PASS**, UTC `2026-10-08T02:59:15.8325262Z`. Eşikteki zombi güvenli iç noktaya tamamlandı ve sonraki istasyonlarda korundu. Ayrıca **6 EditMode evre testi PASS** (UTC `02:57:54`): uzun karede evre atlamama, yaklaşma/kararma/karanlık/açılmada terminal ölüm ve başarısız kalkıştan sonra atamaya gitmeme. Bunlar hasar kapısını atlayıp karanlıkta öldürme anlamına gelmez; saf evre otoritesinin terminal davranışını sınar.

Kullanıcının bildirdiği kırmızı kayıt gerçekti: Play kapanışında ayrı mermi izi nesnesi önce yok olunca `ShotTracer.OnDisable` silinmiş `LineRenderer`'a erişiyordu. Kapanışta varlık kontrolü eklendi; son Play giriş/test/çıkışında Console **0 hata / 0 uyarı**. Test kodundaki geçici NUnit derleme hatası da giderildi. Normal saldırı yeniden gözlendi: oyuncusuz B vagonunda 7 aktif zombi ve kapı 20 → 0; ilk 10 s yaklaşmada spawn olmaması tasarım gereğidir. Bot rakip henüz yoktur.

Tekrar çalıştırma: HUD `Check 10 fast station transitions`; EditMode `Wait Your Turn → Run → Run Flow Tests`. Ayrıntılı çıktılar Git dışında `Logs/RunSandboxReport.json` ve `Logs/RunFlowTests.xml`.

Para, mağaza, farklı silahlar, turret, dron, botlar ve disk/bulut kaydı henüz uygulanmadı. Bunların korunması bu testte kanıtlanmış değildir. Aynı oyuncu/vagon kimliklerinin korunması sonraki bileşenlerin bağlanacağı temeldir. Beş vagon ve gerçek altı kapılı geometriler M4b; kullanıcı tempo/şans/onarım fırsatı değerlendirmesi bekleniyor.
