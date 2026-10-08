# M7c — Oyuncu dronu

TrainIntegration mağazasında **Companion drone (180 test parası)**. Oyuncuda en fazla bir aktif dron; kapanma efekti bitmeden yenisi alınamaz. Başlangıç 1000 para hâlâ geçici deneme ayarıdır. Ürün mevcut ShopService işlem/bağlam/ücret korumalarını kullanır; karanlıkta ve ölümde alışveriş kapalıdır.

## Sorumluluk ve kalıcılık

- `DroneController` Defenses modülündedir; yalnız Combat/Combat.Unity bağımlılığı taşır. Tren akışı, joystick ve cüzdan bilmez. Sahibi, hedef kayıt listesi, yerel hacim ve duraklatma bilgisi dışarıdan verilir.
- `RunDrone` koşuya bağlar: yeni koşu/ölümde temizler, atamada aynı aktörü sahibinin yeni vagonuna taşır, mermiyi korur; karanlıkta silah/uçuş/arama saati durur. Görünür yolculukta savaş sürer.
- `DronePresentation` değiştirilebilir model, rotor, küçük salınım, iz ve patlama görselidir; hasar vermez. Tek aktör sahnede hazırdır; satın alma/patlama sırasında Instantiate/Destroy yoktur. Turretler kendi vagonlarında kalır.
- Ortak `HitscanWeapon`, `NearestVisibleTarget`, `RadialDamage`, Health ve öldürme ödülü kullanılır. Ödül kaynağı oyuncunun yaşam kimliğidir. Havuz hedefinin yaşam kuşağı kontrol edilir.

## Davranış

`Inactive → Following → Searching/Diving → Retiring → Inactive`.

Takip hedefi oyuncunun konumu + vagon eksenlerinde küçük kaymadır; iç hacme sınırlandırılır ve yumuşatılır. Ateş için **oyuncuya en yakın, dronun kendi namlusundan görüşü açık** zombi seçilir. Metal atışı keser, kapı ve yan cam açıklıkları geçirir. Arkadaş gövdesi ortak silah kurallarıyla gerçek ışını engelleyebilir.

Son mermiden sonra hedef **drona en yakın, düz uçuş hacmi açık** zombidir. `DroneFlight` tekrar kullanılan 64'lük sorgu tamponlarıyla küre taraması yapar; bedenler uçuşu engellemez, metal ve camın fiziksel sınırı engeller. Taşma veya başlangıçta engelle örtüşme halinde geçiş reddedilir. Dron kapalı kapıdan/camdan/duvardan geçmez; 2,1 m duvar üstünden dolaşma veya karmaşık rota arama bu sürümde yoktur.

Hedef ölürse ya da kapı onarılıp kapanırsa rota yeniden değerlendirilir. Son mermide başlayan **tek sonlu arama/dalış süresi** yeniden hedef seçimiyle uzamaz. Vagon ataması uçuş kökünü yeni sahibine taşır, dalışı aramaya çevirir ve kalan süreyi korur. Süre dolarsa hasarsız kapanır. Ulaşınca tek patlama: metal arkasına ve arkadaşlara hasar yok, aynı yaşayan hedefe tek hasar; görsel efekt mekanik yarıçap değildir.

## Ayarlar

`Assets/Game/Content/Defenses/Drone.asset` ve `DroneWeapon.asset`; fiyat `Content/Economy/RunShop.asset`.

| İlk deneme değeri | Değer |
| --- | --- |
| Mermi / yedek | 60 / 0 |
| Hasar / ateş aralığı / menzil | 3 / 0,45 sn / 9 m |
| Kök uçuş yüksekliği / takip kayması X,Z | 1,95 m / 0,4 ve 0,35 m |
| Takip yumuşama / azami takip hızı | 0,22 sn / 7 m/sn |
| Hedef seçim aralığı / uçuş yarıçapı | 0,2 sn / 0,14 m |
| Kamikaze hızı / toplam arama-dalış süresi | 6 m/sn / 4 sn |
| Temas uzaklığı / patlama yarıçapı / hasar | 0,3 m / 1,8 m / 18 |
| Görsel salınım genliği / frekansı | 0,05 m / 1,6 Hz |

Değerler yayın dengesi değildir. Geçersiz silah/sonsuz yedek/negatif veya sonlu olmayan kural satın almayı kapatır. Model ölçüsü ve namlu sözleşmesi [MODEL_CONTRACT.md](MODEL_CONTRACT.md), üretilen tablo [drone-model-dimensions.csv](generated/drone-model-dimensions.csv).

## Kurulum ve doğrulama

`Wait Your Turn / Defenses / Install Drone In Integration` (Ctrl+Shift+F1), eksikse bir kez mevcut sahneye bağlar; geometri/NavMesh yeniden kurulmaz. Beş vagon oluşturucu da aynı kurulumu çağırır. F8 Play kabulü gerçek alışveriş, cam atışı, takip, karanlık, 10 atama, sınırlı mermi/kamikaze/ödül, kapalı kapı, ölüm ve aynı aktörü yeniden kullanmayı denetler. Hızlandırılmış bir mermili tanımlar yalnız fixture içinde kopyadır; sonunda özgün ayarlar geri yüklenir.

PC sonuçları [m7c-pc-acceptance.txt](generated/m7c-pc-acceptance.txt) içine alınır. Android, uzun oturum ve gerçek oyuncu his/dengesi ayrıca değerlendirilecek. Sonraki aşama Mod/Bot; botlar ve uzak vagon simülasyonu bu işte uygulanmadı.

Kapı hizalama düzeltmesi de bu adıma dahildir: ilk saldırı yeri merkez, sonraki ikisi iki yandır. Zombi genel menzile girince erkenden durmak yerine ayrılan yere 0,24 m yaklaşır; saldırırken kapının normaline döner. Açık kapıdan ortak serbest NavMesh geçişi korunur.
