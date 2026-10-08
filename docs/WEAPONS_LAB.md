# M5 — Ortak silah ve mermi laboratuvarı

2026-10-08. Beş vagonlu teknik aşama kullanıcı tarafından kabul edildi. `TrainSandbox` aynı evre, kapı, can ve vagon sistemlerini kullanarak genişletilir; mağaza veya turret/dron bu aşamada kurulmaz.

## Kurulum ve deneme

`Wait Your Turn → Weapons → Prepare Five Wagon Weapons`, ardından Play. Mevcut sahnede dört silah düğmesi lab erişimidir; satın alma/silah kilidi anlamına gelmez. Yeni koşu tabancayla başlar. Silah değiştirme mermi eklemez. Mermi bitince oyuncu başka silahı seçebilir; otomatik tabancaya dönüş şu anda yoktur.

| Deneme tanımı | Saçma başına hasar | Atış aralığı | Menzil | Şarjör | Yedek | Dolum |
| --- | --- | --- | --- | --- | --- | --- |
| Pistol | 2 | 0.35 s | 10 | 8 | Sınırsız | 1.2 s |
| SMG | 2 | 0.12 s | 10 | 24 | 72 | 1.5 s |
| Rifle | 5 | 0.28 s | 14 | 20 | 60 | 1.8 s |
| Shotgun | 2 × en çok 6 saçma | 0.85 s | 8 | 6 | 18 | 2 s |

Bunlar denge kararı değil, Inspector'dan değişebilen başlangıç değerleridir (`Assets/Game/Content/Weapons`). Yedek, şarjörün dışındaki sayıdır; örneğin SMG 24 + 72 = 96 toplam atışla başlar. D19 kullanıcı kararı: tabanca sınırsız yedek, diğerleri sınırlı. Tüm türler yedek varsa şarjör bitince otomatik doldurur; son şarjör kısmen dolabilir. Pompalı tetik başına bir mermi harcar, 9° konide altı saçma gönderir; merkez saçma doğrudan hedefe gider. Dağılım Unity global Random durumunu değiştirmez.

## Sorumluluklar

- `WeaponDefinition`: tasarım verisi. Canlı mermi veya süre asset'e yazılmaz.
- `WeaponSpec/WeaponState` (Combat.Core): doğrulanan değişmez kurallar, şarjör/yedek ve oyun saatiyle dolum/ateş süresi. Unity bağımlılığı yoktur.
- `HitscanWeapon` (Combat.Unity): sahip, ekipman seçimi, her tanımın ayrı canlı durumu, ortak resolver ve hasar context'i. Player/Run/Enemy/Shop modüllerine referans vermez. Tek tanımlı savunma da bu bileşeni kullanabilir; gerçek savunma entegrasyonu M7'de doğrulanacak.
- `HitscanResolver`: görüş ve isabet aynı katman maskesiyle en yakın fiziksel engeli seçer. Sahibin colliderları ve triggerlar dışarıda kalır; dost/neutral gövde hasar almasa da arkasına atış geçmez. Saçma başına tek collider sonucu işlenir; aynı hedefteki ikinci collider hasarı katlamaz. Farklı saçmalar aynı zombiye kasıtlı olarak vurabilir.
- `AutoAim`: hareket girdisinden bağımsız en yakın görüşü açık hedef seçimi; gerçek atışı ortak silaha bırakır. Mevcut sol joystick/WASD korunur.
- `ShotTracer/MuzzleFlash`: hasar vermeyen sunum. En çok 16 çizgi ve tek tekrar kullanılan flash; atış başına Instantiate/Destroy yoktur. Satın alınmış Hovl `Flash 1` görselinden yeni kozmetik prefab türetilir; kaynak asset korunur, türetilen kopyada callback scriptleri/collider/Rigidbody/ışık kaldırılır, particle stop action kapatılır. ProjectileMover bağlanmaz.

Silah değiştirmek global tetik beklemesini atlatamaz. Ekipmanda olmayan silahın başlamış dolumu da görünür oyun saatinde sürer; karanlıkta tüm dolumlar durur. RunDriver ortak silaha pause yetkisi verir; doğrudan `TryFire` çağrısı da karanlıkta reddedilir. Aynı oyuncu nesnesi taşındığından seçili silah ve tüm mermi durumları vagon atamasında korunur. `ResetWeapon` yalnızca açık yeni koşu/lab sıfırlamasıdır; mağaza satın alımı için kullanılmamalıdır.

Nav/şeffaf geçiş maskesi geometriye ait olduğundan silah tanımlarında kopyalanmaz. Resolver 64 isabetlik tekrar kullanılan tampon taşır. Tampon dolarsa sonuçlar sırasız olabileceği için atış güvenli biçimde engellenir; duvar atlanmaz. Bu sınırdaki çok yoğun collider geometrisi ileride profilde ayrıca değerlendirilir.

`HitscanPistol` → `HitscanWeapon` dosya/tip geçişinde script `.meta` GUID'i korunur. Mevcut iki/tek vagon sahneleri tanım dizisi bağlanmadıysa önceki 8 mermilik tabanca varsayılanını kullanır; eski serialized `pistol` alan adları referansları korumak için tutulur. Yeni ortak API `RunDriver.Weapon` olarak adlandırılır.

## Doğrulama kaydı

13 EditMode silah testi PASS, UTC `2026-10-08 03:51:28`: sınırlı yedek/kısmi son şarjör, sınırsız tabanca, düşük FPS'de birikmeyen tetik, donmuş dolum, geçersiz veri/saat, ekipman değişiminde mermi/cooldown, compound collider ve saçma maliyeti, katman/duvar/dost engeli, pause/ölü sahip ve koni/Random kontrolü. İlk fizik fixture'ında hedef kökü ile collider yüksekliği farklı kurulduğu için bir test başarısız oldu; kök/aim noktası gerçek oyuncu-zombi düzenine göre hizalanıp tekrar geçildi.

Tekrar: `Wait Your Turn → Weapons → Run Weapon Tests`; rapor Git dışında `Logs/WeaponTests.xml`. Sahne kontrolleri: HUD `Check all weapons / glass / walls` ve `Check 10 fast station transitions`.

Gerçek TrainSandbox geometride dört profil için cam/duvar/alt panel, saçma bildirim sayısı ve tetik başına tek mermi kontrolü PASS (UTC `03:52:41.6924239Z`). Kozmetik flash ve çizgiler açıkken kapı canı değişmedi. Git dışı rapor `Logs/TrainSandboxWeaponsReport.json`.

10/10 hızlandırılmış geçiş tekrar PASS (UTC `03:53:08.4632224Z`): SMG seçimi, 23/24 şarjör ve 72 yedek; aynı oyuncunun 85 canı/yaşamı, beş ayrı kapı, iç/eşik zombi kalıcılığı, dış temizlik, karanlık saat/hareket/onarım/hasar ve terminal ölüm korunur. Rapor `Logs/TrainSandboxReport.json`. Önceki 60-agent yük testi bu turda tekrarlanmadı; AI/nav/pool kapasitesi değişmedi. Saf RunFlow ve can kuralları değişmediği için eski testler tekrar koşturulmadı.

Normal otomatik tabanca/SMG çatışması da gözlendi; bu denge veya cihaz hissi kabulü değildir. Son Play kontrolünde Console 0 hata/0 uyarı. Derlemede eski builder/lab koduna ait deprecated Find API uyarıları vardı; bu aşamada ilgisiz nav builder temizliği yapılmadı.

Mobil ergonomi, yeni efektlerin Android maliyeti, son silah modelleri/sesler ve denge kullanıcı/cihaz değerlendirmesi gerektirir. Bu aşama gerçek turret/dron, ekonomi veya altı kapılı üretim vagonunu tamamlanmış saymaz.
