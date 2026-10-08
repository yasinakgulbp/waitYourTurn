# Görsel hedef ve gerçek vagon entegrasyonu

2026-10-08 kullanıcı hatırlatması. Yeni oturumlarda GAME_DESIGN ve ROADMAP ile birlikte okunur. Bu belge test sahnesinin alışkanlıkla nihai ürün kabul edilmesini önler. Aşağıdaki denetim uygulama öncesinin kaydıdır; güncel uygulama ve kanıt [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md) içindedir.

M6a artık `TrainIntegration` sahnesinde uygulanmıştır: referansın gerçek vagon modeli/oranları ve oyuncu kamerası, her vagonda altı kapı, iki kesintisiz platform, gerçek iç hacim ve geometriye ait doğuş/güvenli konumlar. İki ve beş vagon PC kabulü geçti; beş vagonda 10 geçiş ve 60 düşmanlık havuz sınırı doğrulandı. Kullanıcının yeni kadraj/oynama hissi değerlendirmesi ve birleşimin Android kontrolü henüz yapılmadı. Bot politikası aşağıda hâlâ açık uygulamadır.

## Değişmeyen hedef

Görsel/yerleşim referansı **Assets/Scenes/SampleScene 2.unity** içindeki ilk prototiptir: uzun dikdörtgen vagonlardan oluşan tren, oyuncuyu izleyen kamera ve trenin iki yanındaki istasyon zeminlerinden gelen zombiler. Her vagonda altı kapı hedefi vardır. Kare, tek kapılı, tek taraflı TrainSandbox nihai görünüm veya üretim geometrisi değildir. Sonraki modeller/ışık/efekt çalışması bu referansın üzerine yapılır. Mobil girdi, otomatik nişan/ateş ve hasarla kırılan kapılar için güncel kullanıcı kararları geçerlidir.

Referans sahnede kullanılan mevcut içerikler arasında `Assets/ueni istasyon/yeni vagon/300.fbx` ve `Assets/ueni istasyon/Cevre2.fbx` bulunur. Yerleşim/oranlar bu sahneden alınmalı; laboratuvarın 8×8 zemini ve 10 birim vagon aralığı üretim ölçüsü kabul edilmemeli. İlk prototip korunur; yeni mekanikler kontrollü bir entegrasyon sahnesinde bu yerleşime bağlanır.

Kod/sahne incelemesi: referans Main Camera perspektif, kaydedilmiş FOV 40, X eğimi 70°; `CameraFollow` oyuncunun X konumunu izler ve Y/Z konumunu korur. Sahnede Cinemachine Brain de vardır. Bunlar kaydedilmiş referans değerlerdir, bu incelemede canlı kamera sonucu yeniden oynatılmadı. Entegrasyonda kamera hareketinin tek sahibi seçilmeli; eski CameraFollow ve yeni takip birlikte aynı kamerayı sürmemeli. Nihai kadraj kullanıcı değerlendirmesiyle doğrulanır; laboratuvarın ortografik çapraz kamera açısı otomatik hedef değildir.

## 2026-10-08 uygulama öncesi kod denetimi: genel temel ve lab varsayımları

Ortak Health/hasar, silah/mermi, kapı dayanıklılığı/onarımı, evre saati ve havuz yaşam döngüsü yeniden kullanılacak temeldir. Geometriye bağımlı aşağıdaki parçalar ayrıca uyarlanmalıdır; tek kapı testleri bunların üretim uyumunu kanıtlamaz.

| Mevcut parça | Somut varsayım | Gerçek geometride yapılacak iş |
| --- | --- | --- |
| `EnemyPool` / `EnemyBrain` | Havuz tek DoorController alır; doğan zombi o kapıya bağlanır | Vagon kapıları/erişilebilir giriş adayları doğuş bağlamından seçilir. Aynı vagona ait birden fazla kapı ve iki taraf desteklenir; kapı hasarı/slotları birbirine karışmaz. |
| `EntryPortal.IsInside` / `EnemyBrain.OnBoard` | Kapının iç tarafındaki yarı düzlem içeri girme/kalma kararında kullanılır | Portal eşiği ile gerçek vagon iç hacmi farklı kavramlar olmalı. Karşı taraftaki istasyonun yalnız bir kapının iç tarafında kalması zombiyi trene binmiş saymamalı. Kalkış üyeliği gerçek iç hacim/geçişle doğrulanmalı. |
| `EnemyBrain.Spawn` / takip | Bekleme ofsetinin bir parçası dünya -Z yönünde; iç hedef ilgili kapı düzlemine göre sınırlandırılır | Döndürülmüş/karşı taraf kapılarında bekleme yerel portal eksenlerinden hesaplanmalı; vagon içi hedefler bütün vagonun yürünebilir alanına uymalı. |
| `WagonRuntime.SpawnOutside` / `TrySafePoint` | Sabit negatif Z doğuşu ve sabit 17 iç aday | İki taraflı istasyon spawn çapaları ve gerçek vagona ait güvenli iç noktalar sahne/veriden gelmeli. Kapı, duvar, dolu nokta ve yanlış nav adası ayrıca doğrulanmalı. |
| `RunPresentation` / `RunSandboxBuilder` | Kamera vagon merkezini izler; lab platformu, ray konumu ve dekor ölçüleri sabittir | Kamera oyuncu hareketi + vagon atamasını referansa uygun izlemeli. İki tarafın görselleri gerçek yerleşimden kurulmalı; ray/travers ölçeği gerçek trenle eşleşmeli. Sunum collider/nav taşımamalı. |
| `RunDriver` / hedef bağlamı | Tek insan ölümü koşuyu bitirir; diğer vagonda insan yoksa zombi içeride bekler | Bot aşamasında vagon aktörü/eleme politikası eklenmeli. Bot da hareket/hasar/ateş/onarım kurallarını kullanmalı; bir bot ölümü bütün yarışı bitirmemeli. |

`MovementArea` yerel dikdörtgen sınır verisini destekler; gerçek ölçülerle kurulmalıdır. `WagonRuntime.Doors` dizisi ve en yakın onarım kapısı seçimi vardır; dizinin varlığı çok kapılı enemy hedef seçiminin tamamlandığı anlamına gelmez. Yeni geometride NavMesh yeniden bake edilir; eski baked veriyi kopyalayıp zemini görsel olarak uzatmak kabul edilmez.

## M6a için seçilen entegrasyon kabulü

Mevcut küçük laboratuvarlar regresyon için korunur. Daha fazla lab dekoru veya mağaza/turret/bot özelliğinden önce **gerçek yerleşime yakın oynanabilir entegrasyon** hazırlanır:

1. Referanstan bir uzun dikdörtgen vagon, altı kapı, iki tarafta istasyon alanı, uygun collider/hareket alanı ve buna ait baked NavMesh. Oyuncu takibi/kadrajı referansla karşılaştırılır.
2. Her iki taraftan eşzamanlı doğuş; kapı başına bağımsız hasar/onarım/giriş. Kapalı camdan atış, dolu panel/duvar engeli ve oyuncunun içeride kalması bütün kapılarda doğrulanır. En yakın kapı duvarın arkasındaysa erişilebilir giriş seçimi bozulmamalı.
3. En az iki gerçek oranlı vagonla yaklaşma → savunma → kalkış → karanlık → yeniden atama. Her iki tarafta dışarıda kalanlar, içeridekiler ve iki yöndeki eşik durumları ayrı doğrulanır. Güvenli oyuncu doğuşu, sınırlı havuz ve vagon kalıcılığı korunur; ardından beş vagona uygulanır.

Bu kabul geçmeden tek taraflı lab PASS sonuçları çok kapılı/iki taraflı oyun kabulü olarak sunulmaz. Sonraki M6 spawn/tür/bütçe işi bu geometri verisini kullanır. Sanat cilası daha sonra yapılabilir; doğru yerleşim, kamera davranışı ve giriş topolojisi sanat cilasına ertelenmez.

## Komşu vagon botlarının sırası

Botlar unutulmadı ve henüz uygulanmadı. Sıra: M6a gerçek vagon/iki taraf → M6 spawn ve savaş ölçeği → M7 ekonomi/savunmalar → Mod/Bot aşaması. Tam Battle botu bu ortak kurallarla hareket eder, hedef seçer, ateş eder, kapı onarır ve alışveriş yapar; üç beceri profili ve eleme/sıralama ayrıca doğrulanır. Basit bot kontrolü uygun olduğunda daha erken küçük bir deneme olabilir; bu henüz seçilmiş yeni bir aşama değildir. Şimdiki boş vagonlar gelecekteki bot deneyiminin temsilcisi sayılmaz.

## Uygulama öncesi incelemenin sınırı

Bu adım kaynak kodu, referans sahne verilerini ve plan belgelerini karşılaştırdı. Runtime kodu/sahneler değiştirilmedi; yeni geometri veya kamera tamamlanmış sayılmadı, yeni Play/Android testi yapılmadı. İlgili lab varsayımları somut olarak kaydedildi ve M6a sıradaki zorunlu entegrasyon kabulü yapıldı.
