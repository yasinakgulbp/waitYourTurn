# Wait Your Turn — geliştirme kaydı

Güncelleme: 2026-10-08. Doküman dili Türkçe; kod, tip adları ve commit mesajları İngilizce.

Amaç: mobil zombi savunma oyununu küçük, doğrulanmış adımlarla geliştirmek; yeni mekanikler eklenirken önceki mekaniklerin korunmasını sağlamak.

**TrainSandbox nihai oyun değildir.** İlk prototip yerleşim/oynanış referansıdır; güncel fiziksel oran ve kamera hedefi D23 kullanıcı görselleridir. Yeni geliştirmeden önce [görsel hedef ve entegrasyon denetimi](VISION_AND_INTEGRATION.md) ile [güncel ölçek](VISUAL_SCALE.md) okunur. `TrainIntegration` beş vagonlu PC teknik kabulünü geçti. Bağlama kuralları, test kanıtı ve açık değerlendirmeler [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md) içindedir. M6 spawn/tür/bütçe sistemi PC kabulüne ulaştı; güncel ayarlar ve doğrulama sınırları [SPAWNING.md](SPAWNING.md) içindedir. Sıradaki uygulama M7a cüzdan/ödül/mağaza; Android kalabalık profili açık kalır.

**D23 güncel görsel/ölçek hedefi:** kullanıcının üç yeni referansı eski prototipteki oranları düzeltir. Şimdiki TrainIntegration sade 12 × 4,2 vagon, yönü okunabilen kapsüller, 4–6 kapı varyantları ve ekran oranına uyarlanan perspektif kullanır. Güncel ölçü/model bağlama ve referanslar [VISUAL_SCALE.md](VISUAL_SCALE.md) içindedir; ilk prefabın oranları ve altı-kapı zorunluluğu artık hedef değildir.

## Okuma sırası

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
