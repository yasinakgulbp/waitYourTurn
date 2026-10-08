# Görsel hedef, fiziksel ölçek ve kapı varyantları

2026-10-08 kullanıcı referansları: [1](references/train-visual-target-01.jpeg), [2](references/train-visual-target-02.jpeg), [3](references/train-visual-target-03.jpeg). Bunlar yapay zekâ görselleridir; kamera, oran, görünür tehdit alanı ve ilerideki ışık atmosferini anlatır. Resimde görülen dron, eşya, UI metni, kapı onaran kişiler veya pencere davranışı yeni bir mekanik kararı değildir. D15 gereği atış kapı camından geçer, duvardan geçmez.

İlk prototip oyun yerleşimi/davranış referansı olarak korunur. Eski modelin fiziksel oranlarını birebir koruma şartı yeni görsellerle kaldırıldı. `TrainIntegration` artık hedef oranlarını sınayan sade, değiştirilebilir blok modeller kullanır. Nihai sanat yapılmaz.

## İlk deneme ölçüleri

| Parça | Ölçü / kural |
| --- | --- |
| Vagon iç zemini | 12 × 4,2 birim; tüm kapı varyantlarında aynı |
| Tam yan duvar / engel | 2,1 birim; kapı hareket engeli 2,2 |
| Kapı açıklığı | 1,45 birim; dış çiftlerin X'i ±3,8, orta açıklık X=0 |
| Vagon arası boşluk | 1 birim |
| Oyuncu ve normal zombi | Boy 1,7; çap 0,6; fizik/nav kökü scale=1 |
| Atış yüksekliği | Mevcut çekirdek: namlu 0,9, hedef 0,85; alt panel 0–0,6 |
| Görsel ön duvar | 1,15 yüksekliğinde kesit; fizik duvarı tam yükseklikte |
| Kamera | Perspektif FOV 35, X eğimi 55°; X takibi vagon merkezine göre en fazla ±0,65 |

Bu değerler tasarım denemesidir, değişmez gerçek dünya zorunluluğu veya son denge değildir. Kökü scale ederek oynanış küçültülmez. Model/animasyon sonradan `Replaceable ... visuals` çocuklarına bağlanır; hasar colliderı, CharacterController, NavMeshAgent, Health ve silah kökte kalır. Görsel çocuklar collider/AI/root motion sahibi değildir. Kapsüllerin burun/üst çizgisi ön yönü gösterir. Yeni normal model bu hacme oturur; büyük zombi türü ayrı nav/collision/kapı uyumu gerektirir.

## 4–6 kapı

`Assets/Game/Content/WagonLayouts` içindeki `WagonLayoutDefinition` varlıkları ölçü, kapı maskesi ve kapı canını taşır. Altı slot sırası: güney sol, kuzey sol, güney orta, kuzey orta, güney sağ, kuzey sağ. İlk varyantlar:

- SixDoor: iki yanda üçer; can 10.
- FiveDoor: kuzey orta kapı yok; can 20.
- FourDoor: her iki orta kapı yok; can 40.

Dayanıklılık ve kapı sayısı ayrı ayarlardır; test değerleri final zorluk dengesi değildir. Kaldırılan kapı gerçek duvar olur; portal, kapı canı ve spawn çapası üretilmez. Inspector'dan veri değiştikten sonra **Build Five Wagons** ile collider/NavMesh/sahne yeniden oluşturulur. Bu, elde düzenlenmiş sahneyi koruyan güncelleme aracı veya oyun sırasında kapı açıp kapatma sistemi değildir. Beş-vagon dizilimi 4/5/6/5/4; başlangıç ortadadır, böylece iki komşu görülebilir. Rastgele sonraki atama kuralları korunur.

## Ekran ve sunum

`RunPresentation` kameranın tek sahibidir. `WagonCameraFraming` korunan hacmin sekiz köşesini perspektif ve ekran oranına göre sığdırır. Tüm vagon, komşu uçlar ve iki taraftaki yakın yaklaşma alanı korunur; uzaktaki ikinci spawn sırasının her pikselinin görünmesi şart değildir. Dar ekranlarda mesafe artar, geniş ekranda dikey görüş korunur. HUD cihazın `Screen.safeArea` alanına göre ölçeklenir; silah düğmeleri altta, tanı üstte. Touch joystick HUD alanında başlamaz. Bu geçici HUD, son UI değildir.

Gece atmosferi için okunabilir koyu platform/açık vagon ve sıcak giriş işaretleri kullanılır; tek gölgesiz ana ışık + ambient vardır. Çoklu gerçek zamanlı kapı ışığı, bloom veya post-process eklenmedi. Nihai aşamada sıcak iç ışık / soğuk dış ortam, zombilerin okunurluğu ve flaşlar cihaz bütçesiyle değerlendirilir. Vagon/nav/spawn/collider sabit; istasyon/ray/çevre yalnız görsel köklerden hareket eder.

## Doğrulama sınırı

Kamera matematiği 16:9, 20:9, 4:3 ve 3:4 oranlarında korunan hacim testiyle kontrol edilir. Bu, bütün telefon/tabletlerde dokunmatik ergonomi ve FPS kanıtı değildir; gerçek cihaz kontrolü ayrıca yapılır. F10 kabulü kapı varyantlarında onarım/cam, kapatılmış orta duvarlar ve mevcut istasyon/kalıcılık/havuz kurallarını sınar. Önceki küçük geometri PASS sonuçları bu yeni ölçünün kanıtı sayılmaz.

2026-10-08 doğrulaması:

- Can/onarım/geometri/yerleşim grubu 38/38 PASS (`Logs/HealthAndRepairTests.xml`, 07:11:00 UTC).
- Yolculuk/kamera grubu 15/15 PASS (`Logs/JourneyTests.xml`, 07:10:52 UTC); dört ekran oranında sekiz köşenin korunması dahil.
- Beş-vagon kabulü PASS (`Logs/TrainIntegration-5.txt`, 07:13:38 UTC): 4/5/6 kapılı varyantlarda onarım ve cam, eksik orta kapıların duvar olması, iki taraflı giriş/eşik güvenliği, oyuncu sınırı, 10 istasyon geçişinde tüm vagonların ziyaret edilmesi ve can/SMG/mermi/iç zombi/kapı hasarı kalıcılığı; 60 canlı havuz sınırı.
- Unity 16:9 yatay telefon ve 4:3 tablet önizlemelerinde tüm aktif vagon, yakın yaklaşma alanları ve HUD kadrajı gözle kontrol edildi. Ortadaki başlangıç vagonunda iki komşunun uçları görünür; dizinin ucunda doğal olarak tek komşu vardır. Son Play Console: 0 hata, 0 uyarı.

Android build, gerçek çentik/dokunmatik ergonomi, uzun oturum ve nihai sanat bu revizyonda test edilmedi. Eski lablar ve orijinal prototip değişmedi. Güncel sahne `Train-5.asset` bake'ini kullanır; başka vagon sayısı için ilgili Build komutu o sayının bake'ini yeniden üretir.
