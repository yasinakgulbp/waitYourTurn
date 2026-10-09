# Wait Your Turn — geliştirme kaydı

Güncelleme: 2026-10-09. Doküman dili Türkçe; kod, tip adları ve commit mesajları İngilizce.

Amaç: mobil zombi savunma oyununu küçük, doğrulanmış adımlarla geliştirmek; yeni mekanikler eklenirken önceki mekaniklerin korunmasını sağlamak.

**TrainSandbox nihai oyun değildir.** İlk prototip yerleşim/oynanış referansıdır; güncel fiziksel oran ve kamera hedefi D23 kullanıcı görselleridir. Yeni geliştirmeden önce [görsel hedef ve entegrasyon denetimi](VISION_AND_INTEGRATION.md) ile [güncel ölçek](VISUAL_SCALE.md) okunur. `TrainIntegration` beş vagonlu PC teknik kabulünü geçti. Bağlama kuralları, test kanıtı ve açık değerlendirmeler [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md) içindedir. M6 spawn/tür/bütçe ve M7a cüzdan/ödül/mağaza PC teknik kabulüne ulaştı; güncel ayarlar ve sınırlar [SPAWNING.md](SPAWNING.md) ve [ECONOMY.md](ECONOMY.md) içindedir. M7b sabit turret de PC teknik kabulüne ulaştı; [DEFENSES.md](DEFENSES.md) ayar/sahiplik/test kaydıdır. M7c dron da PC kabulüne ulaştı; [DRONE.md](DRONE.md) davranış, ayar ve test kaydıdır. Mod/Bot ilk PC kabulü PASS: dört yerel bot, üç profil ve D33 eleme/sonuç; BOTS.md. M8a yerel kayıt temeli eklendi; güncel sözleşme PERSISTENCE.md; Android kalabalık profili/mağaza ergonomisi açık kalır.

**D23 güncel görsel/ölçek hedefi:** kullanıcının üç yeni referansı eski prototipteki oranları düzeltir. Şimdiki TrainIntegration sade 12 × 4,2 vagon, yönü okunabilen kapsüller, 4–6 kapı varyantları ve ekran oranına uyarlanan perspektif kullanır. Güncel ölçü/model bağlama ve referanslar [VISUAL_SCALE.md](VISUAL_SCALE.md) içindedir; ilk prefabın oranları ve altı-kapı zorunluluğu artık hedef değildir.

**D27/D28 güncel durum:** boş/elenmiş vagonlara yeni saldırı üretilmez; içeridekiler korunur. Rastgele atamada geçerli adaylar içinde düşmana en yüksek mesafe ve ayarlanabilir 2 sn hasar koruması kullanılır. 28 spawn/yolculuk + 40 can/onarım testi, F11 30 geçiş, F9 mağaza ve F10 birleşik çekirdek PC kabulü PASS. [Kısa kanıt](generated/occupancy-arrival-pc-acceptance.txt). Bu D27/D28 kanıtı botlardan önce alınmıştır; güncel bot uygulaması BOTS.md içindedir. Gelir önerileri [MONETIZATION.md](MONETIZATION.md) içinde, servisler eklenmedi.

## Okuma sırası

**Son kullanıcı düzeltmesi D35:** açık kapıda kalabalık baskısına karşı gerçek kapsül sınırı ve insan/bot için yumuşak yürüyüş dönüşü. Davranış, Inspector ayarı ve F3 yeniden üretim testi [PLAYER_MOVEMENT.md](PLAYER_MOVEMENT.md) içinde. M8b sıradaki aşama olarak kalır.

**Güncel adım M8a yerel kayıt:** [PERSISTENCE.md](PERSISTENCE.md). İnsan/bot yön davranışı ve aynı koşuya devam temeli uygulanır. Editor F5 kaydet / F6 devam; mobil build otomatik kayıt/devam kullanır. PC kabulü ile Android süreç kapatma kabulü ayrı tutulur; M8 denge/oyuncu arayüzü ve bulut tamamlanmış değildir.

Güncel Mod/Bot: [BOTS.md](BOTS.md) — ortak bileşenler, profiller, sıralama, atama, testler ve açık maliyet işleri.

1. [Oyun tasarımı](GAME_DESIGN.md): oyuncunun deneyimi, kurallar, henüz kesinleşmemiş detaylar.
2. [Mimari](ARCHITECTURE.md): sorumluluklar, bağımlılıklar ve kritik teknik kurallar.
3. [Kararlar](DECISIONS.md): kararlaştırılanlar, öneriler ve açık sorular.
4. [Yol haritası](ROADMAP.md): sıralı geliştirme işleri ve kabul koşulları.
5. [Prototip başlangıç kaydı](BASELINE.md): mevcut yerel prototipin kaynak kontrolüne alınan durumu ve doğrulama sınırları.
6. [Navigasyon laboratuvarı](NAVIGATION_LAB.md): ilk uygulama, kontroller, doğrulama ve açık işler.
7. [Can/hasar laboratuvarı](COMBAT_LAB.md): ortak kurallar ve M2 kanıtı.
8. [Tek vagon oynanışı](GAMEPLAY_LAB.md): M3 entegrasyonu, kontroller ve açık cihaz değerlendirmesi.
9. [Vagon/koşu laboratuvarı](RUN_LAB.md): M4a iki vagon, M4b beş vagon; evre, kalıcılık, atama ve doğrulama sınırları.
10. [Silah laboratuvarı](WEAPONS_LAB.md): M5 ortak hitscan/pellet, dört silah tanımı, sınırlı yedek ve doğrulama.
11. [Görsel hedef ve entegrasyon](VISION_AND_INTEGRATION.md): referans prototip, somut lab varsayımları, M6a kabulü ve bot sırası.
12. [Gerçek vagon entegrasyonu](TRAIN_INTEGRATION.md): sahne/kurulum, geometri sahipliği, iki taraflı saldırı ve PC kabulü; ilk M6a geçmişi ayrıca korunur.
13. [Güncel görsel ölçek](VISUAL_SCALE.md): üç kullanıcı referansı, fiziksel boyutlar, 4–6 kapı, model bağlama ve ekran uyumu.
14. [Model üretimi ve bağlama sözleşmesi](MODEL_CONTRACT.md): pivot/eksen/birimler, net kapı-pencere açıklıkları, metal hacimleri, kesit görünümü ve sahneden üretilen ölçü CSV'si. D24 ile yan pencerelerden ateş edilir, metal çerçevelerden edilmez.

15. [Ekonomi ve mağaza](ECONOMY.md): gerçek ölüm ödülü, sahiplik/yaşam kuşağı, satın alma/refill/yerel onarım, Inspector ayarları ve PC kanıtı.
16. [Gelir ve kalıcı ekonomi taslağı](MONETIZATION.md): kullanıcının reklam/IAP fikirleri, doğrulanmış örnekler, önerilen sade ürün/ödül yapısı ve henüz seçilmeyen kararlar.

## Mevcut durum

Güncel D24: TrainIntegration iki taraflı ateş edilebilir yan pencereler kullanır; metal çerçeve/dikmeler atışı durdurur, beden/nav sınırı sürer. Model ölçü/pivot ve açıklık kaynağı [MODEL_CONTRACT.md](MODEL_CONTRACT.md). Son 39 kural testi ve beş-vagon F10 kabulü PASS; ayrıntılar VISUAL_SCALE.md.

- Uygulama başladı: ayrı NavigationSandbox sahnesinde kontrollü NavMesh geçişi kuruldu. Açık tasarım kararları `DECISIONS.md` içinde.
- 2026-10-07 revizyonu: M1 erken cihaz ölçümü, M3 minimum mobil kontrol/tabanca ve oynanış değerlendirmesi, M4a iki vagon → M4b beş vagon. Yeniden kullanım/kapsam sınırları ve erken tasarım kararları `ROADMAP.md` içinde.
- İncelenen Unity: 6000.6.4f1; kurulu AI Navigation: 2.0.14. Paket güncellemesi bu planın parçası değil.
- Referans/build sahnesi: `Assets/Scenes/SampleScene 2.unity`; ayrı test sahneleri: NavigationSandbox, CombatSandbox ve GameplaySandbox (`Assets/Game/Scenes`).
- Prototip: 5 rastgele oyuncu doğuş noktası, 6 düşman doğuş noktası, 7 saniyede bir üretim, 40 saniyede sahne yenileme.
- Eski prototip vagon zorluğunu kapı zamanlamasıyla oluşturur. Yeni GameplaySandbox'ta hasarlı kapı, onarım, oyuncuya saldırı/ölüm ve otomatik tabanca bağlıdır; istasyon/etap ilerlemesi henüz yoktur.
- İlk incelemede sahne yenileme/vagon değişimi görüldü. Altı zombi prefabındaki fazladan AudioListener bileşenleri M0 düzeltmesinde kaldırıldı; AudioSource ve eski oynanış scriptleri korundu.
- Plan öncesindeki yerel prototip değişiklikleri ayrı başlangıç commitine alındı. Eski Windows buildleri/arşivi yerelde korundu ve Git dışında tutuldu; plan commitine dahil edilmedi.

## Her geliştirme adımının çalışma biçimi

1. Yol haritasından tek bir aşama/iş seç; bağımlılıkları ve açık kararları oku.
2. Hangi davranışın değişeceğini ve aşamanın kabul koşullarını belirle.
3. Önce küçük test sahnesinde kur, ardından hedef oyun sahnesine entegre et.
4. İlgili mantık testlerini ve Play kontrollerini yap; performansı gereken aşamada cihazda ölç.
5. `ROADMAP.md` durumunu ve aşağıdaki kaydı güncelle. Kanıt olmadan işi tamamlandı işaretleme.
6. Yalnızca bu işe ait dosyaları commit et. Kullanıcının mevcut çalışmalarını izinsiz dahil etme. Commitler küçük ve İngilizce olsun.

Örnek commit: `feat: add damageable doors and repair interaction`.

Yeni bir oturumda bu dizin, güncel kod, Git durumu ve Unity sahnesi birlikte okunur. Plan belgesi tek başına uygulamanın kanıtı değildir. Değişen tasarım kararının gerekçesi `DECISIONS.md` içine eklenir; eski karar sessizce değiştirilmez.

## İlerleme kaydı

2026-10-09 M8a/D34: hedef yokken insan/bot gerçekleşen hareket yönüne bakar; görünür hedef dönüşte önceliklidir. Yerel kayıt katılımcı/vagon/eleme, can/para/mermi/onarım, savunmalar, spawn ve atama RNG durumunu korur. 104 ilgili kural testi ile F4/F7/F9/F10 PC kabulleri PASS; kayıt JSON'undaki dronsuz durum ayrıca doğrulandı. Editor F5 kaydet / F6 devam; Android yaşam döngüsü, bulut, sonuç arayüzü ve denge açık. [Kayıt sözleşmesi](PERSISTENCE.md), [PC kanıtı](generated/m8a-pc-acceptance.txt).

| Tarih | İş | Durum | Doğrulama / sonraki adım |
| --- | --- | --- | --- |
| 2026-10-07 | Prototip incelemesi | Tamamlandı | Kod/prefab incelemesi ve bir Play döngüsü; kaynak kod değiştirilmedi. |
| 2026-10-07 | Oyun tasarımı, mimari ve aşamalı plan | Hazır; açık kararlar var | İlk iş M0: prototip kaydı ve küçük navigasyon test sahnesi hazırlığı. |
| 2026-10-07 | Mevcut yerel prototipi kaynak kontrolüne alma | Tamamlandı | Kaynak/asset/ayarlar ayrı commit; eski dağıtım çıktıları Git dışında. M0/M1 test sahnesi henüz kurulmadı. |
| 2026-10-07 | Yararlı dış inceleme önerilerini plana işleme | Tamamlandı | Belgeler güncellendi; oynanış kodu ve Unity sahneleri değişmedi. Yeni aşama kabul koşulları henüz doğrulanmadı. |
| 2026-10-07 | NavMesh geçişi ve paralel kapı hatları | M1 lab kabulü tamamlandı | Son A54 APK'sı 10/10 PASS; 100/100 varış 36.25 saniye, eşzamanlı giriş 2. Kaba yük örnekleri yaklaşık 60 FPS; CPU örneği kaydedildi. Üretim entegrasyonu M3'te. Ayrıntılar NAVIGATION_LAB.md içinde. |
| 2026-10-07 | M0 geliştirme düzeni ve Android lab buildi | M0 tamamlandı | Referans sahne/zombi üretimi görüldü; AudioListener düzeltmesi ayrı commit. Android Development APK ve kimlik geri yüklemesi başarılı. A54'e kurulum/çalıştırma doğrulandı. |
| 2026-10-07 | Vagon kenarında kapı ve kesintisiz kalabalık geçişi | Tek-agent kontrolü 10/10; kalabalık hedef yerleşimi açık | Köprü/ikili link kaldırıldı; ortak NavMesh + carving. 30 agentta 29 hedefe varış, bir agent vagon içinde avoidance nedeniyle takıldı. M3/M6 hedef yönetimi işi; ayrıntı NAVIGATION_LAB.md. Rutin testler kullanıcı talebiyle PC'de. |
| 2026-10-07 | M2 ortak can, hasar ve ölüm | Tamamlandı | 14 EditMode testi geçti; hitscan, kapı 8+2, oyuncu iyileşmesi ve ölüm sinyali Play'de doğrulandı. COMBAT_LAB.md. Sıradaki M3: kapı kırılma/onarımı, enemy brain, minimum girdi/tabanca ve pool. |
| 2026-10-07 | M3 tek vagon mekaniği entegrasyonu (cam revizyonundan önce) | PC teknik kontrolü geçti; kullanıcı/cihaz değerlendirmesi açık | O sürüm 10/10 PASS: kapı hasarı/giriş, oyuncuya saldırı, iptal edilen/tam onarım, eşik güvenliği, pool yaşamı ve önceki kapı görüş engeli. Sol joystick/otomatik ateş ve otomatik reload kullanıcı tercihiyle kuruldu; GAMEPLAY_LAB.md. Android bu turda denenmedi. |
| 2026-10-07 | M3 kullanıcı geri bildirimi: cam kapı ve vagon sınırı | PC odaklı birleşik kontrol PASS | D15: sağlam/onarılmış camdan dış zombiye gerçek atış; alt panel/duvar engeli. D16: oyuncu sağlam/kırık/onarılmış kapıdan dışarı çıkamaz, zombi girişi korunur; görev için alan kaldırılabilir. Gerçek zombi kapı hasarı → kırılma → giriş → yakınlık onarımı da geçti. UTC 18:53:55; GAMEPLAY_LAB.md. Önceki 10 döngü bu revizyonda yeniden çalıştırılmadı; Android denenmedi. |

Reklam/IAP'nin ekonomi rolü M7 öncesinde tasarlanır; servis entegrasyonu ve yayınlama çekirdek oynanış/cihaz performansı doğrulandıktan sonraki aşamalardır.

2026-10-08: M4a iki vagonlu koşu sahnesi kuruldu. 10/10 geçiş + 6 saf evre testi PASS; normal iki-vagon saldırısı ve Play/GameOver kapanışında 0 hata doğrulandı. Kapı/oyuncu/mermi/iç-zombi kalıcılığı kanıtlandı; kullanıcı PC değerlendirmesini kabul etti. Battle/botsuz mod ve en az üç bot beceri profili tasarım hedefine eklendi, bot uygulaması yapılmadı. Google Play bulut kaydı aday; entegrasyon M8/servis aşamasında. Ayrıntı [RUN_LAB.md](RUN_LAB.md).

2026-10-08 M4b: aynı akış beş ayrı vagonla `TrainSandbox` sahnesinde kuruldu; yerleşim/kapı başlangıç canları TrainLayout verisinden uygulanır. 10/10 geçişte beş vagonun tümü ziyaret edildi ve durumlar korundu; 60 eşzamanlı zombiyle kısa PC yük kontrolü PASS (son kontrol: ortalama 4.82 ms/p95 7.38 ms, Editor tanısı). Kullanıcı teknik aşamayı kabul etti; gerçek altı kapılı tren, botlar ve bu sürümün Android kontrolü henüz yok. Geliştirmede tutarlı tekrar Play için standart domain/sahne reload açıldı; istasyon geçişleri hâlâ reload gerektirmez.

2026-10-08 M5: ortak HitscanWeapon/Resolver ve saf WeaponState kuruldu; dört tanım TrainSandbox'a bağlandı. Tabanca sınırsız yedek, SMG/tüfek/pompalı sınırlı yedek (D19). 13 EditMode testi, dört silahın gerçek cam/duvar/alt panel kontrolü ve seçili SMG/şarjör/yedeğin korunduğu 10/10 geçiş PASS. Hovl flash kozmetik kopyadan tekrar kullanılır, hasar ortak resolver'dan gelir. Kullanıcı/Android hissi ve denge açık; mağaza/turret/dron henüz yok. Ayrıntılar WEAPONS_LAB.md.

2026-10-08 D20 onarım devamı (`18a9bab`): hasarlı sağlam kapıya da süreli onarım; varsayılan oyuncu hasarında devam, kapı hasarında sıfırlama, iki ayrı ayar. 32 can/onarım vakası ve TrainSandbox kapı/işgal kontrolü PASS. Ayrıntı [GAMEPLAY_LAB.md](GAMEPLAY_LAB.md).

2026-10-08 D02 yolculuk sunumu tamamlaması: yalnız travers hareketi eksikti; istasyon platformu sabit colliderın görünümünden ayrıldı, yol zemini/çevresi de hareket eder. İstasyon döngülenmez, duruşta hizalanır; yeni istasyon tam karanlıkta yerleştirilir. 11 evre/hareket vakası ve beş-vagonda 10 geçiş PASS; collider/nav/spawn sabitliği denetlendi. D21 ile trenin iki tarafından saldırı hedefi kayda alındı; gerçek iki taraflı/altı kapılı geometri M6'da açık. Ayrıntı ve model bağlama kuralları [RUN_LAB.md](RUN_LAB.md), sahiplik [ARCHITECTURE.md](ARCHITECTURE.md), kesin kararlar [DECISIONS.md](DECISIONS.md). Android ve nihai görsel his değerlendirmesi ayrıca yapılacak.

2026-10-08 D22 hedef denetimi: ilk prototipin kamera/yerleşimi ile yeni kod karşılaştırıldı. Tek kapı enemy bağı, portal yarı düzleminden üyelik, sabit doğuş/iç noktalar ve vagon merkezine bağlı kamera gerçek geometri için açık uyarlamalardır. M6a ayrı ve sıradaki kabul olarak öne alındı; bu entegrasyon geçmeden yeni mağaza/turret/bot özelliğine geçilmeyecek. GAME_DESIGN'daki eski kırık-kapıya-özel onarım açıklaması D20 ile düzeltildi. Bu incelemede runtime/sahne değişikliği ve yeni Play testi yoktur. Ayrıntı [VISION_AND_INTEGRATION.md](VISION_AND_INTEGRATION.md).

2026-10-08 M6a uygulaması: kaynak prototip/prefab ve eski lablar korunarak TrainIntegration kuruldu. Referans model/oran, oyuncu X kamerası, vagon başına altı kapı, iki kesintisiz platform ve ayrı görsel hareket kullanılır. WagonGeometry gerçek iç hacim/doğuş/güvenli adayları sahiplenir; havuz erişilebilir kendi-vagon girişini seçer. Dar gerçek kapılar için ayrı Train Zombie agent türü ve yeni ortak NavMesh bake kullanılır; eski Humanoid ayarı korunur. İki-vagon 06:05:32 UTC, beş-vagon 06:08:40 UTC PC kabulü PASS: 10 geçiş, durumlar, iki taraflı eşik temizliği ve 24/60 düşmanlık havuz sınırı. 34 can/onarım/geometri ve 11 yolculuk testi PASS. Eşikte yakın oyuncuya saldırarak duran zombi sorunu, önce geçişi tamamlama önceliğiyle giderildi. Kullanıcı kadraj/his, Android/uzun oturum ve botlar açık; sıradaki uygulama M6. Ayrıntı [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md).

2026-10-08 D23 görsel ölçek revizyonu: üç kullanıcı referansı repoya alındı; TrainIntegration 12 × 4,2 vagon, 1,7 boyunda yön işaretli kapsüller, 4/5/6/5/4 kapı ve uyarlanan perspektifle yeniden kuruldu. Kaldırılan kapılar gerçek duvardır; görsel çocuklar fizik/nav kökünden ayrıdır. 38 can/onarım/geometri/yerleşim ve 15 yolculuk/kamera testi PASS. Beş-vagon F10 kabulü 07:13:38 UTC PASS: bütün kapı varyantları, 10 istasyon geçişinde durum kalıcılığı ve 60 düşman havuz sınırı. 16:9 ve 4:3 Unity önizlemeleri kontrol edildi; Android ve nihai sanat/ergonomi bu turda doğrulanmadı. Ayrıntı [VISUAL_SCALE.md](VISUAL_SCALE.md). Sıradaki iş M6.

2026-10-08 D24: kullanıcı iki taraflı yan pencerelerden de atış istedi. Net camlar ayrı metal colliderlarla çevrildi; sürekli beden/NavMesh sınırı korunur. 4/5/6 kapı varyantları 10/9/8 yan pencere taşır; modelci için MODEL_CONTRACT ve builder tarafından üretilen yerel konum/boyut CSV'si eklendi. 39 kural testi PASS; 08:03:45 UTC F10 kabulünde 46 pencerenin tamamı dört gerçek silah, metal kenar/alt/üst panel ve eğik raylarla geçti. Kapı/onarım/eşik, 10 geçişte durumlar ve 60 düşman havuz sınırı PASS. İlk uç camın uç duvarına taşması kenar testinde yakalanıp giderildi. Android ölçülmedi; sıradaki uygulama M6.

2026-10-08 M6: StationDefinition programları ve normal/hızlı/dayanıklı ortak profiller TrainIntegration’a bağlandı. Global/vagon/kare/kuyruk sınırları, kontrollü havuz ısınması, sonlu nav/çakışma denemesi ve dolu-vagon adaleti eklendi. EditMode ve Play kanıtları, başlangıç ayarları ve Android doğrulama sınırı [SPAWNING.md](SPAWNING.md) içinde; son PC rapor zamanları aşağıdaki M6 kaydında güncellenir.

M6 son PC kanıtı: 15:33:53 UTC 26/26 kural/yolculuk testi; 15:35:13 UTC 30 istasyon/dokuz yeniden kullanım/5 yolcu PASS; 15:36:24 UTC kapı/cam/dört silah/10 geçiş/60 beden F10 PASS. Ham yerel raporların kısa kopyası docs/generated/m6-pc-acceptance.txt. Android kalabalık profili ve gerçek uzun oturum açık; sıradaki uygulama M7a.


2026-10-08 pencere/hız düzeltmesi: bütün uç köşeler metal; ara camlar %5–9 daraltıldı (çerçeve/dikme 0,32/0,18 m). Normal/hızlı/dayanıklı hızları 2,1/3,0/1,45 m/sn. Model sözleşmesi ve ölçü CSV'si yenilendi. 16:00:18 UTC F10 PASS: 26 yan camda dört silah, metal çerçeveler ve her vagonun dört uç bölümünde düz/eğik raylar; beden/nav sınırı, kapılar ve 10 geçiş korunur. Kısa kanıt: docs/generated/window-refinement-pc-acceptance.txt. Android bu düzeltmede tekrar ölçülmedi. Sıradaki aşama M7a.


2026-10-08 M7a: saf Wallet/ShopService/KillRewards, RunEconomy bağlayıcısı ve değiştirilebilir ShopHud TrainIntegration'a bağlandı. D25 savaşta gerçek zamanlı mağaza, D26 ilk silah açma/tekrar mermi tamamlama uygulanır. 16:18:42 UTC 11 ekonomi, 16:19:17 UTC 15 silah testi PASS. 16:25:44 UTC F9 gerçek öldürme/tek ödül, iyileşme/yerel onarım/silah-refill, 10 atamada kalıcılık, eski bağlam/karanlık/ölüm reddi ve Restart sıfırlaması PASS. 16:26:30 UTC F10 önceki 26 cam/dört silah/kapı/onarım/10 geçiş/60 beden kabulü PASS; Play Console 0 hata/0 uyarı. Ayarlar ve bağımlılıklar [ECONOMY.md](ECONOMY.md), kanıt [generated/m7a-pc-acceptance.txt](generated/m7a-pc-acceptance.txt). Fiyatlar geçici; Android ergonomisi/uzun oturum, reklam-IAP kararları ve kayıt açık. Sıradaki M7b sabit turret; ardından M7c dron ve Mod/Bot.

2026-10-08 D27/D28: kullanıcı boş vagonlarda biriken zombiler ve atamada ani ölüm bildirdi. Canlı savunucu kaydı üretim uygunluğunu belirler; boş/elenmiş akışlar o istasyon için kapanır, mevcut yolcular silinmez. Atamada en yüksek düşman mesafeli geçerli aday + Inspector'dan ayarlanabilir 2 sn koruma kuruldu. 17:30:01 UTC 28 spawn/yolculuk ve 17:30:11 UTC 40 can/onarım testi PASS. 17:31:13 UTC F11 boş/boşaltılan/elenen vagon, test savunucularıyla global bütçe, dokuz profil yeniden kullanımı, 30 geçişte beş yolcu ve koruma süresi PASS. 17:32:15 UTC F9 mağaza/ödül/ölüm, 17:33:02 UTC F10 26 cam/dört silah/kapı/onarım/10 geçiş/60 beden PASS; Console 0 hata/0 uyarı. [Kanıt](generated/occupancy-arrival-pc-acceptance.txt). Gerçek bot/uzak simülasyon/Android kontrolü yapılmadı. Kullanıcının reklam/IAP fikirleri ve kaynaklı sade ekonomi önerileri MONETIZATION.md içine alındı; fiyat, kalıcı ödül ve servis kararı uygulanmadı. Sıradaki M7b sabit turret.

2026-10-08 M7b: normal/gelişmiş sabit turretler, vagon başına her türden 2 sınırı, 50/80 sonlu mermi ve bitişte tek alan hasarı kuruldu. 20 silah/patlama + 12 ekonomi testi ve F12 turret kabulü PASS; kurulu gövde yanından giriş, eski vagon ödülü, karanlık/ölüm kapısı ve aktör yeniden kullanımı doğrulandı. Model pad/gövde/namlu ölçüleri CSV ve MODEL_CONTRACT içinde. Son kanıt generated/m7b-pc-acceptance.txt; Android/nihai denge açık. D29 turret kararları, D30 Unity Ads tercihi ve IAP ertelemesi kayda alındı. Sıradaki M7c dron, sonra Mod/Bot.

2026-10-08 başlangıç dengesi: kullanıcı turret alacak paraya ulaşamadan öldüğünü bildirdi. İlk iki istasyona daha zayıf Intro profili, 4,5 sn doğuş aralığı, 4 canlı sınırı ve 12 para ödülü verildi. Sekiz öldürme 96 para ile normal turret fiyatını karşılar. Üçüncü istasyon ve sonrası değişmedi; sonraki kullanıcı oynanışıyla denge değerlendirilecek. Ayarlar SPAWNING.md içinde.

**D31 son kullanıcı revizyonu:** turret sabit pad aramadan oyuncunun bulunduğu konuma kurulur; küçük çarpışma alanı, sahibin içinden yürümesi. Duvarlar eşit yükseklikte, dikey pencere metalleri daha geniş; güçlü vagon tren başında, sağında kapalı lokomotif. Normal koşuda yeni rastgele tohum/ilk vagon. Deneme başlangıç parası geçici 1000. MODEL_CONTRACT ve DEFENSES güncel uygulamayı anlatır; PC revizyon kanıtı generated/free-placement-train-pc-acceptance.txt. Bu revizyonu M7c dron izledi; güncel sonraki iş Mod/Bot.

2026-10-08 M7c/D32: bir dron/oyuncu, ortak namlu/atış/ödül, sonlu mermi ve engel kontrollü kamikaze; takip, karanlık duraklatma, vagon kalıcılığı ve tek aktörü yeniden kullanma. Kapı saldırı noktası merkezden başlar; zombi yerine ulaşmadan menzilde durmaz, kapıya döner. Model/namlu sözleşmesi ve üretilen ölçü CSV'si eklendi. PC kanıtı generated/m7c-pc-acceptance.txt. Sonraki uygulama Mod/Bot; Android, uzun oturum ve yayın dengesi açık.

2026-10-08 Mod/Bot/D33: beş vagonda insan + dört yerel bot, üç veri odaklı beceri profili, ortak hareket/ateş/onarım/alışveriş kuralları ve ayrı can/para/mermi. RunMatch eşsiz canlı ataması ve eleme sırasını sahiplenir; insan ölünce koşu biter, yalnız insan kalınca kazanır, aynı karede elenenler aynı sırayı paylaşır. Battle/Solo seçimi yeni koşu başlatır. 20:32:17 UTC 33 Run testi; 20:33:55 UTC F7 gerçek bot davranışı/onarım/otomatik atış/ödül/eleme/10 atama PASS. F8–F12 önceki kontroller PASS, Console 0 hata/0 uyarı. [Bot sistemi](BOTS.md), [PC kanıtı](generated/bots-pc-acceptance.txt). Uzak vagonların yaklaşık simülasyonu, Android, uzun yarış dengesi ve kullanıcı oynanış değerlendirmesi açık; sıradaki M8 kayıt/devam.
